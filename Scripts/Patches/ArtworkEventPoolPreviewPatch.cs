using System;
using System.Collections.Generic;
using System.Reflection;
using HarmonyLib;
using KomeijiKoishi.Cards;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.CardPools;
using MegaCrit.Sts2.Core.Models.Cards;

namespace KomeijiKoishi.Patches
{
    [HarmonyPatch]
    public static class ArtworkEventPoolPreviewPatch
    {
        private static readonly Type[] ArtworkPreviewCards =
        {
            typeof(TrueArtwork_Koishi),
            typeof(TrueArtworkLiberated_Koishi)
        };

        public static IEnumerable<System.Reflection.MethodBase> TargetMethods()
        {
            var getter = AccessTools.PropertyGetter(typeof(CardPoolModel), nameof(CardPoolModel.AllCards));
            if (getter != null)
            {
                yield return getter;
            }
        }

        public static void Postfix(CardPoolModel __instance, ref IEnumerable<CardModel> __result)
        {
            if (__instance is not EventCardPool || __result == null)
            {
                return;
            }

            var cards = new List<CardModel>(__result);
            var changed = false;

            foreach (var cardType in ArtworkPreviewCards)
            {
                if (ContainsCardType(cards, cardType))
                {
                    continue;
                }

                var card = GetCardModel(cardType);
                if (card != null)
                {
                    cards.Add(card);
                    changed = true;
                }
            }

            if (changed)
            {
                __result = cards;
            }
        }

        private static CardModel? GetCardModel(Type cardType)
        {
            foreach (var method in typeof(ModelDb).GetMethods(BindingFlags.Public | BindingFlags.Static))
            {
                if (method.Name != nameof(ModelDb.Card) || !method.IsGenericMethodDefinition || method.GetGenericArguments().Length != 1)
                {
                    continue;
                }

                var parameters = method.GetParameters();
                if (parameters.Length != 0)
                {
                    continue;
                }

                return method.MakeGenericMethod(cardType).Invoke(null, null) as CardModel;
            }

            return null;
        }

        private static bool ContainsCardType(IEnumerable<CardModel> cards, Type cardType)
        {
            foreach (var card in cards)
            {
                if (card.GetType() == cardType)
                {
                    return true;
                }
            }

            return false;
        }
    }
}
