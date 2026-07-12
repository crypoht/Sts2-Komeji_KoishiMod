using System.Collections.Generic;
using System.Linq;
using HarmonyLib;
using KomeijiKoishi.Cards;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Runs;

namespace KomeijiKoishi.Patches
{
    [HarmonyPatch(typeof(CardCreationOptions), nameof(CardCreationOptions.GetPossibleCards))]
    public static class ArtworkRewardPoolPatch
    {
        public static void Postfix(Player player, ref IEnumerable<CardModel> __result)
        {
            __result = __result.Where(card => card is not ArtworkStage_Koishi || card is Artwork_Koishi);
        }
    }
}
