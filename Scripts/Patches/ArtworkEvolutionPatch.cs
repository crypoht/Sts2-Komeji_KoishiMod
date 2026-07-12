using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using HarmonyLib;
using KomeijiKoishi.Cards;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Nodes.CommonUi;

namespace KomeijiKoishi.Patches
{
    [HarmonyPatch(typeof(CardCmd), nameof(CardCmd.Upgrade), typeof(IEnumerable<CardModel>), typeof(CardPreviewStyle))]
    public static class ArtworkEvolutionPatch
    {
        public sealed class UpgradeState
        {
            public List<CardModel> Cards { get; }

            public CardPreviewStyle OriginalStyle { get; }

            public UpgradeState(List<CardModel> cards, CardPreviewStyle originalStyle)
            {
                Cards = cards;
                OriginalStyle = originalStyle;
            }
        }

        public static void Prefix(ref IEnumerable<CardModel> cards, ref CardPreviewStyle style, ref UpgradeState __state)
        {
            List<CardModel> cardList = cards.ToList();
            __state = new UpgradeState(cardList, style);
            cards = cardList;

            if (HasPendingArtworkEvolution(cardList))
            {
                style = CardPreviewStyle.None;
            }
        }

        public static void Postfix(UpgradeState __state)
        {
            List<IArtworkEvolutionCard> evolvingCards = __state.Cards
                .OfType<IArtworkEvolutionCard>()
                .Where(IsReadyToEvolve)
                .ToList();

            if (evolvingCards.Count > 0)
            {
                TaskHelper.RunSafely(EvolveCards(evolvingCards, __state.OriginalStyle));
            }
        }

        private static bool HasPendingArtworkEvolution(IEnumerable<CardModel> cards)
        {
            return cards.OfType<IArtworkEvolutionCard>().Any(IsPendingEvolution);
        }

        private static bool IsPendingEvolution(IArtworkEvolutionCard card)
        {
            CardModel model = (CardModel)card;
            return model.CurrentUpgradeLevel + 1 >= model.MaxUpgradeLevel
                && !model.UpgradePreviewType.IsPreview()
                && model.Pile != null
                && model.IsTransformable;
        }

        internal static bool IsReadyToEvolve(IArtworkEvolutionCard card)
        {
            CardModel model = (CardModel)card;
            return card.ShouldEvolve
                && !model.UpgradePreviewType.IsPreview()
                && model.Pile != null
                && model.IsTransformable;
        }

        internal static async Task EvolveCards(IEnumerable<IArtworkEvolutionCard> cards, CardPreviewStyle style)
        {
            CardPreviewStyle transformStyle = style == CardPreviewStyle.None
                ? CardPreviewStyle.HorizontalLayout
                : style;

            foreach (IArtworkEvolutionCard artwork in cards)
            {
                CardModel original = (CardModel)artwork;
                if (original.Pile == null || !original.IsTransformable)
                {
                    continue;
                }

                await CardCmd.Transform(original, artwork.CreateEvolution(), transformStyle);
            }
        }
    }

    [HarmonyPatch]
    public static class ArtworkEvolutionAfterDeckAddPatch
    {
        public static MethodBase? TargetMethod()
        {
            return AccessTools.GetDeclaredMethods(typeof(CardPileCmd))
                .FirstOrDefault(method =>
                {
                    if (method.Name != nameof(CardPileCmd.Add))
                    {
                        return false;
                    }

                    ParameterInfo[] parameters = method.GetParameters();
                    return parameters.Length >= 5
                        && parameters[0].ParameterType == typeof(IEnumerable<CardModel>)
                        && parameters[1].ParameterType == typeof(CardPile)
                        && parameters[2].ParameterType == typeof(CardPilePosition)
                        && parameters[3].ParameterType == typeof(AbstractModel)
                        && parameters[4].ParameterType == typeof(bool);
                });
        }

        public static void Postfix(Task<IReadOnlyList<CardPileAddResult>> __result, CardPile newPile)
        {
            if (newPile.Type != PileType.Deck)
            {
                return;
            }

            TaskHelper.RunSafely(EvolveAddedArtworkCards(__result));
        }

        private static async Task EvolveAddedArtworkCards(Task<IReadOnlyList<CardPileAddResult>> resultTask)
        {
            IReadOnlyList<CardPileAddResult> results = await resultTask;
            List<IArtworkEvolutionCard> evolvingCards = results
                .Where(result => result.success)
                .Select(result => result.cardAdded)
                .OfType<IArtworkEvolutionCard>()
                .Where(ArtworkEvolutionPatch.IsReadyToEvolve)
                .ToList();

            if (evolvingCards.Count == 0)
            {
                return;
            }

            await ArtworkEvolutionPatch.EvolveCards(evolvingCards, CardPreviewStyle.HorizontalLayout);
        }
    }
}
