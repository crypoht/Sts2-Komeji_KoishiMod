using System.Reflection;
using BaseLib.Abstracts;
using BaseLib.Extensions;
using BaseLib.Patches.Content;
using HarmonyLib;
using KomeijiKoishi.Ancients;
using KomeijiKoishi.Config;
using KomeijiKoishi.Modifiers;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Events;
using MegaCrit.Sts2.Core.Rooms;
using MegaCrit.Sts2.Core.Runs;

namespace KomeijiKoishi.Patches
{
    [HarmonyPatch(typeof(ActModel), nameof(ActModel.GenerateRooms))]
    [HarmonyAfter("BaseLib")]
    public static class AncientWeightPatch
    {
        private static readonly FieldInfo RoomsField = AccessTools.Field(typeof(ActModel), "_rooms");

        public static void Postfix(ActModel __instance, List<AncientEventModel>? ____sharedAncientSubset)
        {
            RunState? runState = CurrentGeneratingRunState.State;
            if (runState == null || runState.Modifiers.Any(m => m is DisableKoishiAncientWeightsModifier) || !runState.Modifiers.Any(m => m is KoishiAncientWeightsModifier))
            {
                return;
            }

            if (HasExternalAncientConfigConflict())
            {
                MegaCrit.Sts2.Core.Logging.Log.Info("[KoishiAncientWeights] 当前先古之民设置存在冲突，已跳过权重池");
                return;
            }

            if (RoomsField.GetValue(__instance) is not RoomSet { HasAncient: true } rooms)
            {
                return;
            }

            List<AncientEventModel> candidates = BuildCandidates(__instance, runState, ____sharedAncientSubset);
            if (__instance.ActNumber() <= 1 && candidates.Count <= 1)
            {
                return;
            }

            AncientEventModel? weightedAncient = runState.Rng.UpFront.WeightedNextItem(candidates, GetWeight);
            if (weightedAncient != null)
            {
                rooms.Ancient = weightedAncient;
            }
        }

        private static List<AncientEventModel> BuildCandidates(ActModel act, RunState runState, List<AncientEventModel>? sharedAncientSubset)
        {
            List<AncientEventModel> candidates = act.GetUnlockedAncients(runState.UnlockState).ToList();

            if (sharedAncientSubset != null)
            {
                foreach (AncientEventModel ancient in sharedAncientSubset)
                {
                    if (candidates.All(a => a.Id != ancient.Id))
                    {
                        candidates.Add(ancient);
                    }
                }
            }

            foreach (CustomAncientModel ancient in CustomContentDictionary.CustomAncients)
            {
                if (candidates.All(a => a.Id != ancient.Id))
                {
                    candidates.Add(ancient);
                }
            }

            return candidates
                .Where(a => IsValidForCurrentAct(a, act))
                .Where(a => !WasAlreadyUsedInPreviousAct(a, act, runState))
                .Where(a => GetWeight(a) > 0f)
                .ToList();
        }

        private static bool IsValidForCurrentAct(AncientEventModel ancient, ActModel act)
        {
            return ancient is not CustomAncientModel customAncient || customAncient.IsValidForAct(act);
        }

        private static bool WasAlreadyUsedInPreviousAct(AncientEventModel ancient, ActModel currentAct, RunState runState)
        {
            int currentActNumber = currentAct.ActNumber();
            return runState.Acts
                .Where(act => act.ActNumber() < currentActNumber)
                .Select(act => act.Ancient)
                .Any(previousAncient => previousAncient != null && previousAncient.Id == ancient.Id);
        }

        private static float GetWeight(AncientEventModel? ancient)
        {
            if (ancient == null)
            {
                return 0f;
            }

            AncientWeights weights = KoishiModConfig.GetAncientWeightsForRun(CurrentGeneratingRunState.State);
            int weight = ancient switch
            {
                MoriyaTwoGods_Koishi => weights.MoriyaTwoGods,
                HakureiReimu_Koishi => weights.HakureiReimu,
                Orobas => weights.Orobas,
                Pael => weights.Pael,
                Tezcatara => weights.Tezcatara,
                Nonupeipe => weights.Nonupeipe,
                Tanx => weights.Tanx,
                Vakuu => weights.Vakuu,
                Darv => weights.Darv,
                _ => GetExternalWeight(ancient)
            };

            return Math.Max(0, weight);
        }

        private static int GetExternalWeight(AncientEventModel ancient)
        {
            string key = AncientProbabilityData.ExternalKey(ancient);
            if (KoishiModConfig.GetExternalAncientWeightsForRun(CurrentGeneratingRunState.State).TryGetValue(key, out int weight))
            {
                return int.Clamp(weight, 0, 10);
            }

            return 3;
        }

        private static bool HasExternalAncientConfigConflict()
        {
            return HasKoishiAncientConfigConflict() || HasTouhouAncientsConfigConflict();
        }

        private static bool HasKoishiAncientConfigConflict()
        {
            Type? configType = AccessTools.TypeByName("KoishiAncient.Config.KoishiConfig");
            if (configType == null)
            {
                return false;
            }

            return GetStaticBoolProperty(configType, "BanKoishi")
                || GetStaticBoolProperty(configType, "ForcedAncient_2");
        }

        private static bool HasTouhouAncientsConfigConflict()
        {
            Type? configType = AccessTools.TypeByName("TouhouAncients.Scripts.TouhouAncientsConfig");
            if (configType == null)
            {
                return false;
            }

            foreach (PropertyInfo property in configType.GetProperties(BindingFlags.Public | BindingFlags.Static))
            {
                if (property.Name.StartsWith("Ban", StringComparison.Ordinal)
                    && property.PropertyType == typeof(bool)
                    && GetStaticBoolProperty(configType, property.Name))
                {
                    return true;
                }

                if (property.Name.StartsWith("ForcedAncient_", StringComparison.Ordinal)
                    && IsNonDefaultForcedAncientOption(property))
                {
                    return true;
                }
            }

            return false;
        }

        private static bool GetStaticBoolProperty(Type type, string propertyName)
        {
            try
            {
                PropertyInfo? property = type.GetProperty(propertyName, BindingFlags.Public | BindingFlags.Static);
                return property?.PropertyType == typeof(bool) && property.GetValue(null) is true;
            }
            catch (Exception e)
            {
                MegaCrit.Sts2.Core.Logging.Log.Error($"[KoishiAncientWeights] Failed to read {type.FullName}.{propertyName}: {e}");
                return true;
            }
        }

        private static bool IsNonDefaultForcedAncientOption(PropertyInfo property)
        {
            try
            {
                object? value = property.GetValue(null);
                if (value == null)
                {
                    return false;
                }

                return Convert.ToInt32(value) != 0;
            }
            catch (Exception e)
            {
                MegaCrit.Sts2.Core.Logging.Log.Error($"[KoishiAncientWeights] Failed to read {property.DeclaringType?.FullName}.{property.Name}: {e}");
                return true;
            }
        }
    }
}
