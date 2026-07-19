using System.Collections.Generic;
using System.Linq;
using HarmonyLib;
using KomeijiKoishi.Cards;
using MegaCrit.Sts2.Core.Factories;
using MegaCrit.Sts2.Core.Models;

namespace KomeijiKoishi.Patches
{
    [HarmonyPatch(typeof(CardFactory), "GetFilteredTransformationOptions")]
    public static class ArtworkTransformPoolPatch
    {
        public static void Postfix(ref CardModel[] __result)
        {
            CardModel[] filtered = __result
                .Where(card => card is not ArtworkStage_Koishi || card is Artwork_Koishi)
                .ToArray();

            if (filtered.Length > 0)
            {
                __result = filtered;
            }
        }
    }
}
