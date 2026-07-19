using System.Collections.Generic;
using System.Linq;
using HarmonyLib;
using KomeijiKoishi.Cards;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Hooks;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Runs;

namespace KomeijiKoishi.Patches
{
    public static class ArtworkPoolFilter
    {
        public static IEnumerable<CardModel> FilterEvolvedArtwork(IEnumerable<CardModel> cards)
        {
            return cards.Where(card => card is not ArtworkStage_Koishi || card is Artwork_Koishi);
        }
    }

    [HarmonyPatch(typeof(CardCreationOptions), nameof(CardCreationOptions.GetPossibleCards))]
    public static class ArtworkRewardPoolPatch
    {
        public static void Postfix(Player player, ref IEnumerable<CardModel> __result)
        {
            __result = ArtworkPoolFilter.FilterEvolvedArtwork(__result);
        }
    }

    [HarmonyPatch(typeof(Hook), nameof(Hook.ModifyMerchantCardPool))]
    public static class ArtworkShopPoolPatch
    {
        public static void Postfix(ref IEnumerable<CardModel> __result)
        {
            __result = ArtworkPoolFilter.FilterEvolvedArtwork(__result);
        }
    }
}
