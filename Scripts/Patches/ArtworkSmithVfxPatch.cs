using System.Collections.Generic;
using System.Linq;
using HarmonyLib;
using KomeijiKoishi.Cards;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Nodes.Vfx;

namespace KomeijiKoishi.Patches
{
    [HarmonyPatch(typeof(NCardSmithVfx), nameof(NCardSmithVfx.Create), typeof(IEnumerable<CardModel>), typeof(bool))]
    public static class ArtworkSmithVfxPatch
    {
        public static bool Prefix(ref IEnumerable<CardModel> cards, ref NCardSmithVfx? __result)
        {
            List<CardModel> filteredCards = cards
                .Where(card => card is not IArtworkEvolutionCard artwork || !artwork.ShouldEvolve)
                .ToList();

            if (filteredCards.Count == 0)
            {
                __result = null;
                return false;
            }

            cards = filteredCards;
            return true;
        }
    }
}
