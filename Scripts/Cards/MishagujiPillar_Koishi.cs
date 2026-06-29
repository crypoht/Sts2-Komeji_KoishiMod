using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using BaseLib.Abstracts;
using BaseLib.Utils;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.CardPools;
using MegaCrit.Sts2.Core.Models.Cards;
using MegaCrit.Sts2.Core.Nodes.CommonUi;
using MegaCrit.Sts2.Core.ValueProps;

namespace KomeijiKoishi.Cards
{
    [Pool(typeof(TokenCardPool))]
    public sealed class MishagujiPillar_Koishi : CustomCardModel
    {
        public MishagujiPillar_Koishi()
            : base(3, CardType.Attack, CardRarity.Token, TargetType.AnyEnemy, true)
        {
        }

        public override string PortraitPath => KoishiImagePaths.CardPortrait(GetType());

        protected override IEnumerable<DynamicVar> CanonicalVars => new List<DynamicVar>
        {
            new DamageVar(17m, ValueProp.Move)
        };

        public override async Task AfterCardDrawnEarly(PlayerChoiceContext choiceContext, CardModel card, bool fromHandDraw)
        {
            if (card != this)
            {
                return;
            }

            Creature? target = null;
            if (base.CombatState != null && base.Owner != null)
            {
                target = base.Owner.RunState.Rng.Shuffle.NextItem(base.CombatState.HittableEnemies);
            }

            await CardCmd.AutoPlay(choiceContext, this, target, AutoPlayType.Default, true, false);
        }

        protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
        {
            ArgumentNullException.ThrowIfNull(cardPlay.Target);

            await DamageCmd.Attack(base.DynamicVars.Damage.BaseValue)
                .FromCard(this)
                .Targeting(cardPlay.Target)
                .WithHitFx("vfx/vfx_attack_blunt")
                .Execute(choiceContext);

            CardModel clone = base.CreateClone();
            CardPileAddResult addResult = await CardPileCmd.AddGeneratedCardToCombat(
                clone,
                PileType.Hand,
                base.Owner,
                CardPilePosition.Bottom
            );
            CardCmd.PreviewCardPileAdd(addResult, 1.2f, CardPreviewStyle.HorizontalLayout);
        }

        protected override void OnUpgrade()
        {
            base.DynamicVars.Damage.UpgradeValueBy(4m);
        }
    }
}
