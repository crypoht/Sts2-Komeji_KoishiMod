using HarmonyLib;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;

namespace KomeijiKoishi.Patches
{
    [HarmonyPatch(typeof(SwipePower), nameof(SwipePower.BeforeDeath))]
    public static class SwipePowerStolenCardUpgradePatch
    {
        public static void Prefix(SwipePower __instance, Creature target)
        {
            if (__instance.Owner != target)
            {
                return;
            }

            CardModel? stolenCard = __instance.StolenCard;
            CardModel? deckCard = stolenCard?.DeckVersion;
            if (stolenCard == null || deckCard == null)
            {
                return;
            }

            int missingUpgradeLevels = stolenCard.CurrentUpgradeLevel - deckCard.CurrentUpgradeLevel;
            if (missingUpgradeLevels <= 0)
            {
                return;
            }

            for (int i = 0; i < missingUpgradeLevels && deckCard.IsUpgradable; i++)
            {
                deckCard.UpgradeInternal();
                deckCard.FinalizeUpgradeInternal();
            }
        }
    }
}
