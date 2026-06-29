using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BaseLib.Abstracts;
using BaseLib.Utils;
using KomeijiKoishi.Pools;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Cards;

namespace KomeijiKoishi.Cards
{
    [Pool(typeof(KoishiCardPool))]
    public sealed class KeeperOfImaginaryPersonality_Koishi : CustomCardModel
    {
        private const string ExhaustCardsKey = "ExhaustCards";

        public KeeperOfImaginaryPersonality_Koishi()
            : base(0, CardType.Skill, CardRarity.Rare, TargetType.Self, true)
        {
        }

        public override string PortraitPath => KoishiImagePaths.CardPortrait(GetType());

        protected override IEnumerable<DynamicVar> CanonicalVars => new List<DynamicVar>
        {
            new DynamicVar(ExhaustCardsKey, 10m),
            new EnergyVar(3),
            new CardsVar(3)
        };

        protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
        {
            if (base.Owner is not Player player)
            {
                return;
            }

            List<CardModel> candidates = new List<CardModel>();
            PileType[] pileTypes = { PileType.Hand, PileType.Draw, PileType.Discard };

            foreach (PileType pileType in pileTypes)
            {
                candidates.AddRange(
                    pileType.GetPile(player).Cards
                        .Where(card => card != this && card != cardPlay.Card)
                );
            }

            int cardsToExhaust = base.DynamicVars[ExhaustCardsKey].IntValue;
            int exhaustedCount = 0;

            for (int i = 0; i < cardsToExhaust && candidates.Count > 0; i++)
            {
                CardModel? card = player.RunState.Rng.Shuffle.NextItem(candidates);
                if (card == null)
                {
                    break;
                }

                candidates.Remove(card);
                await CardCmd.Exhaust(choiceContext, card, false, false);
                exhaustedCount++;
            }

            if (exhaustedCount == cardsToExhaust)
            {
                await PlayerCmd.GainEnergy(base.DynamicVars.Energy.IntValue, player);
                await CardPileCmd.Draw(choiceContext, base.DynamicVars.Cards.BaseValue, player, false);
            }
        }

        protected override PileType GetResultPileTypeForCardPlay()
        {
            if (base.Owner is Player player)
            {
                PileType[] pileTypes = { PileType.Hand, PileType.Draw, PileType.Discard };
                int availableCards = pileTypes
                    .SelectMany(pileType => pileType.GetPile(player).Cards)
                    .Count(card => card != this);

                if (availableCards < base.DynamicVars[ExhaustCardsKey].IntValue)
                {
                    return PileType.Exhaust;
                }
            }

            return base.GetResultPileTypeForCardPlay();
        }

        protected override void OnUpgrade()
        {
            base.DynamicVars.Energy.UpgradeValueBy(1m);
            base.DynamicVars.Cards.UpgradeValueBy(1m);
        }
    }
}
