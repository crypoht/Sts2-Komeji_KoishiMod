using System.Collections.Generic;
using System.Linq;
using HarmonyLib;
using KomeijiKoishi.Config;
using KomeijiKoishi.Pools;
using KomeijiKoishi.Relics;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.RelicPools;
using MegaCrit.Sts2.Core.Multiplayer.Game;
using MegaCrit.Sts2.Core.Random;
using MegaCrit.Sts2.Core.Runs;

namespace KomeijiKoishi.Patches
{
    [HarmonyPatch(typeof(RelicGrabBag), nameof(RelicGrabBag.Populate), typeof(Player), typeof(Rng))]
    public static class KoishiPlayerRelicGrabBagPatch
    {
        [ThreadStatic]
        private static bool? _nestedPopulateIsMultiplayer;

        internal static bool? IsNestedPopulateMultiplayer => _nestedPopulateIsMultiplayer;

        public static bool Prefix(RelicGrabBag __instance, Player player, Rng rng)
        {
            bool isMultiplayer = player.RunState.Players.Count > 1;
            var relics = ModelDb.RelicPool<SharedRelicPool>()
                .GetUnlockedRelics(player.UnlockState)
                .Concat(ModelDb.RelicPool<KoishiSharedRelicPool>().GetUnlockedRelics(player.UnlockState))
                .Concat(player.Character.RelicPool.GetUnlockedRelics(player.UnlockState))
                .Where(relic => !IsMultiplayerOnlyExcluded(relic, isMultiplayer))
                .Where(relic => !KoishiBalanceManager.IsEnabled || relic is not KoishiRock)
                .DistinctBy(relic => relic.Id);

            _nestedPopulateIsMultiplayer = isMultiplayer;
            try
            {
                __instance.Populate(relics, rng);
            }
            finally
            {
                _nestedPopulateIsMultiplayer = null;
            }

            return false;
        }

        internal static bool IsMultiplayerOnlyExcluded(RelicModel relic, bool isMultiplayer)
        {
            return relic is Dataminer_Koishi
                && isMultiplayer;
        }
    }

    [HarmonyPatch(typeof(RelicGrabBag), nameof(RelicGrabBag.Populate), typeof(IEnumerable<RelicModel>), typeof(Rng))]
    public static class KoishiSharedRelicGrabBagPatch
    {
        public static void Prefix(ref IEnumerable<RelicModel> relics)
        {
            List<RelicModel> relicList = relics
                .Where(relic => !IsMultiplayerOnlyExcluded(relic))
                .Where(relic => !KoishiBalanceManager.IsEnabled || relic is not KoishiRock)
                .ToList();
            bool isSharedRelicPoolPopulation = relicList.Any(relic => relic.Pool is SharedRelicPool);
            if (!isSharedRelicPoolPopulation)
            {
                relics = relicList;
                return;
            }

            HashSet<ModelId> existingIds = relicList.Select(relic => relic.Id).ToHashSet();
            foreach (RelicModel relic in ModelDb.RelicPool<KoishiSharedRelicPool>().AllRelics)
            {
                if (!IsMultiplayerOnlyExcluded(relic) && existingIds.Add(relic.Id))
                {
                    relicList.Add(relic);
                }
            }

            relics = relicList;
        }

        private static bool IsMultiplayerOnlyExcluded(RelicModel relic)
        {
            bool isMultiplayer = KoishiPlayerRelicGrabBagPatch.IsNestedPopulateMultiplayer
                ?? RunManager.Instance?.NetService?.Type.IsMultiplayer()
                ?? false;
            return KoishiPlayerRelicGrabBagPatch.IsMultiplayerOnlyExcluded(relic, isMultiplayer);
        }
    }
}
