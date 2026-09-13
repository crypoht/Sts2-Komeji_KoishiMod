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
using MegaCrit.Sts2.Core.Random;
using MegaCrit.Sts2.Core.Runs;

namespace KomeijiKoishi.Patches
{
    [HarmonyPatch(typeof(RelicGrabBag), nameof(RelicGrabBag.Populate), typeof(Player), typeof(Rng))]
    public static class KoishiPlayerRelicGrabBagPatch
    {
        public static bool Prefix(RelicGrabBag __instance, Player player, Rng rng)
        {
            var relics = ModelDb.RelicPool<SharedRelicPool>()
                .GetUnlockedRelics(player.UnlockState)
                .Concat(ModelDb.RelicPool<KoishiSharedRelicPool>().GetUnlockedRelics(player.UnlockState))
                .Concat(player.Character.RelicPool.GetUnlockedRelics(player.UnlockState))
                .Where(relic => !KoishiBalanceManager.IsEnabled || relic is not KoishiRock)
                .DistinctBy(relic => relic.Id);

            __instance.Populate(relics, rng);
            return false;
        }
    }

    [HarmonyPatch(typeof(RelicGrabBag), nameof(RelicGrabBag.Populate), typeof(IEnumerable<RelicModel>), typeof(Rng))]
    public static class KoishiSharedRelicGrabBagPatch
    {
        public static void Prefix(ref IEnumerable<RelicModel> relics)
        {
            List<RelicModel> relicList = relics
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
                if (existingIds.Add(relic.Id))
                {
                    relicList.Add(relic);
                }
            }

            relics = relicList;
        }
    }
}
