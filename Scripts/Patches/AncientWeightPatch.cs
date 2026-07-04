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
using System.Collections;

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

            foreach (AncientEventModel ancient in GetEcoLibCustomAncients())
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
            if (ancient is CustomAncientModel customAncient)
            {
                return customAncient.IsValidForAct(act);
            }

            if (ancient.GetType().FullName == "Koishi.KoishiCode.Ancient.Satori")
            {
                return IsSatoriValidForCurrentAct(act);
            }

            return IsExternalCustomAncientValidForAct(ancient, act);
        }

        private static bool IsSatoriValidForCurrentAct(ActModel act)
        {
            int actNumber = act.ActNumber();
            RunState? state = CurrentGeneratingRunState.State;
            string? characterId = state?.Players?.FirstOrDefault()?.Character?.Id?.Entry;
            return characterId == "KOISHI-KOISHI"
                ? actNumber is 2 or 3
                : actNumber == 3;
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
            return HasKoishiAncientConfigConflict()
                || HasTouhouAncientsConfigConflict()
                || HasEcoKoishiSatoriConfigConflict();
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

        private static bool HasEcoKoishiSatoriConfigConflict()
        {
            Type? configType = AccessTools.TypeByName("Koishi.KoishiCode.Config.KoishiConfig");
            if (configType == null)
            {
                return false;
            }

            try
            {
                bool satoriExtraTreatment = GetStaticBoolField(configType, "SatoriExtraTreatment");
                bool satoriNonKoishiExtraTreatment = GetStaticBoolField(configType, "SatoriNonKoishiExtraTreatment");
                float satoriAct2Chance = GetStaticFloatField(configType, "SatoriAct2Chance", 0.25f);
                float satoriAct3Chance = GetStaticFloatField(configType, "SatoriAct3Chance", 0.45f);
                float satoriNonKoishiAct3Chance = GetStaticFloatField(configType, "SatoriNonKoishiAct3Chance", 0.2f);

                return satoriExtraTreatment
                    || satoriNonKoishiExtraTreatment
                    || Math.Abs(satoriAct2Chance - 0.25f) > 0.0001f
                    || Math.Abs(satoriAct3Chance - 0.45f) > 0.0001f
                    || Math.Abs(satoriNonKoishiAct3Chance - 0.2f) > 0.0001f;
            }
            catch (Exception e)
            {
                MegaCrit.Sts2.Core.Logging.Log.Error($"[KoishiAncientWeights] Failed to read Koishi Satori config: {e}");
                return true;
            }
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

        private static bool GetStaticBoolField(Type type, string fieldName)
        {
            FieldInfo? field = type.GetField(fieldName, BindingFlags.Public | BindingFlags.Static);
            return field?.FieldType == typeof(bool) && field.GetValue(null) is true;
        }

        private static float GetStaticFloatField(Type type, string fieldName, float fallback)
        {
            FieldInfo? field = type.GetField(fieldName, BindingFlags.Public | BindingFlags.Static);
            return field?.FieldType == typeof(float) && field.GetValue(null) is float value
                ? value
                : fallback;
        }

        private static IEnumerable<AncientEventModel> GetEcoLibCustomAncients()
        {
            Type? dictionaryType = AccessTools.TypeByName("EcoLib.Patches.Content.CustomContentDictionary");
            FieldInfo? customAncientsField = dictionaryType?.GetField("CustomAncients", BindingFlags.Public | BindingFlags.Static);
            if (customAncientsField?.GetValue(null) is not IEnumerable customAncients)
            {
                yield break;
            }

            foreach (object? ancient in customAncients)
            {
                if (ancient is AncientEventModel ancientModel)
                {
                    yield return ancientModel;
                }
            }
        }

        private static bool IsExternalCustomAncientValidForAct(AncientEventModel ancient, ActModel act)
        {
            try
            {
                MethodInfo? method = ancient.GetType().GetMethod("IsValidForAct", BindingFlags.Instance | BindingFlags.Public);
                ParameterInfo[] parameters = method?.GetParameters() ?? Array.Empty<ParameterInfo>();
                return method?.ReturnType == typeof(bool)
                    && parameters.Length == 1
                    && parameters[0].ParameterType.IsAssignableFrom(typeof(ActModel))
                    && method.Invoke(ancient, new object[] { act }) is true;
            }
            catch (Exception e)
            {
                MegaCrit.Sts2.Core.Logging.Log.Error($"[KoishiAncientWeights] External ancient {ancient.GetType().FullName} failed IsValidForAct for {act.Id.Entry}: {e}");
                return false;
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
