using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BaseLib.Abstracts;
using BaseLib.Utils;
using KomeijiKoishi.Pools;
using MegaCrit.Sts2.Core.CardSelection;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Extensions;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.CardPools;
using MegaCrit.Sts2.Core.Models.Cards;
using MegaCrit.Sts2.Core.Nodes.CommonUi;

namespace KomeijiKoishi.Cards.Fumo
{
    [Pool(typeof(TokenCardPool))]
    public sealed class KoakumaFumo_koishi : CustomCardModel
    {
        private const int OptionCount = 33;

        public KoakumaFumo_koishi()
            : base(0, CardType.Skill, CardRarity.Token, TargetType.Self, true)
        {
        }

        public override string PortraitPath => $"res://mods/Komeiji_Koishi/images/cards/fumo/{GetType().Name}.png";

        public override IEnumerable<CardKeyword> CanonicalKeywords => new[] { CardKeyword.Exhaust };

        protected override IEnumerable<DynamicVar> CanonicalVars => new List<DynamicVar>
        {
            new CardsVar(OptionCount)
        };

        protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
        {
            List<CardModel> options = GetCandidateCards()
                .ToList()
                .StableShuffle(base.Owner.RunState.Rng.CombatCardGeneration)
                .Take(OptionCount)
                .Select(card => base.Owner.RunState.CreateCard(card, base.Owner))
                .ToList();

            if (options.Count == 0)
            {
                return;
            }

            CardSelectorPrefs prefs = new CardSelectorPrefs(base.SelectionScreenPrompt, 1);
            IEnumerable<CardModel> selectedCards = await CardSelectCmd.FromSimpleGrid(choiceContext, options, base.Owner, prefs);
            List<CardModel> cardsToAdd = selectedCards.Take(1).ToList();
            if (cardsToAdd.Count == 0)
            {
                return;
            }

            IReadOnlyList<CardPileAddResult> results = await CardPileCmd.Add(cardsToAdd, PileType.Deck, CardPilePosition.Bottom, null, false);
            CardCmd.PreviewCardPileAdd(results, 1.2f, CardPreviewStyle.HorizontalLayout);
        }

        protected override void OnUpgrade()
        {
            base.EnergyCost.UpgradeBy(+385);
        }

        private IEnumerable<CardModel> GetCandidateCards()
        {
            IEnumerable<CardPoolModel> pools = ModelDb.AllCharacterCardPools
                .Append(ModelDb.CardPool<ColorlessCardPool>());

            return pools
                .SelectMany(pool => pool.GetUnlockedCards(base.Owner.UnlockState, base.Owner.RunState.CardMultiplayerConstraint))
                .Where(card => card.Type != CardType.Status
                    && card.Type != CardType.Curse
                    && card.Type != CardType.Quest
                    && card.Rarity != CardRarity.Event
                    && card.Rarity != CardRarity.Token)
                .Distinct();
        }
    }
}
