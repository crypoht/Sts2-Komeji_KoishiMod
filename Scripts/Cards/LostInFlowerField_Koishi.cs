using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BaseLib.Abstracts;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Factories;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Nodes.CommonUi; 
using KomeijiKoishi.Pools;
using KomeijiKoishi.Powers;
using KomeijiKoishi.Enums; 
using BaseLib.Utils;

namespace KomeijiKoishi.Cards
{
    [Pool(typeof(KoishiCardPool))]
    public sealed class LostInFlowerField_Koishi : CustomCardModel
    {
        public LostInFlowerField_Koishi() 
            : base(2, CardType.Skill, CardRarity.Rare, TargetType.Self, true) { }

        public override string PortraitPath => KoishiImagePaths.CardPortrait(GetType());

        public override IEnumerable<CardKeyword> CanonicalKeywords => new[] { CardKeyword.Exhaust };

        protected override IEnumerable<DynamicVar> CanonicalVars => new List<DynamicVar> 
        { 
            new DynamicVar("GenerateAmount", 10m),
            new DynamicVar("RetrieveAmount", 2m)
        };

        protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
        {
            var player = base.Owner as Player;
            if (player == null) return;

            await CreatureCmd.TriggerAnim(player.Creature, "Cast", player.Character.CastAnimDelay);

            IEnumerable<CardModel> generatedCards = CardFactory.GetForCombat(
                base.Owner, 
                from c in base.Owner.Character.CardPool.GetUnlockedCards(base.Owner.UnlockState, base.Owner.RunState.CardMultiplayerConstraint)
                where c.Type != CardType.Status && c.Type != CardType.Curse && c.GetType() != this.GetType()
                select c, 
                base.DynamicVars["GenerateAmount"].IntValue, 
                base.Owner.RunState.Rng.CombatCardGeneration
            );

            foreach (CardModel cardModel in generatedCards)
            {
                cardModel.SetToFreeThisCombat();
                
                CardCmd.ApplyKeyword(cardModel, new CardKeyword[] { KoishiKeywords.LostInFlowerField });

                CardPileAddResult addResult = await CardPileCmd.AddGeneratedCardToCombat(cardModel, PileType.Exhaust, cardPlay.Card.Owner, CardPilePosition.Random);
                CardCmd.PreviewCardPileAdd(addResult, 1.2f, CardPreviewStyle.HorizontalLayout);
            }

            await PowerCmd.Apply<LostInFlowerFieldPower>(
                choiceContext,
                player.Creature, 
                base.DynamicVars["RetrieveAmount"].BaseValue, 
                player.Creature, 
                this
            );
        }

        protected override void OnUpgrade()
        {
            base.DynamicVars["GenerateAmount"].UpgradeValueBy(2m);
            base.DynamicVars["RetrieveAmount"].UpgradeValueBy(1m);
        }
    }
}