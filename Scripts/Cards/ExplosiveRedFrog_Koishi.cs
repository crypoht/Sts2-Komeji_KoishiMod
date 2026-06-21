using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using BaseLib.Abstracts;
using BaseLib.Utils;
using KomeijiKoishi.Pools;
using KomeijiKoishi.Powers;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.CardPools;
using MegaCrit.Sts2.Core.Models.Cards;

namespace KomeijiKoishi.Cards
{
    [Pool(typeof(TokenCardPool))]
    public sealed class ExplosiveRedFrog_Koishi : CustomCardModel
    {
        public ExplosiveRedFrog_Koishi()
            : base(1, CardType.Skill, CardRarity.Ancient, TargetType.AnyEnemy, true)
        {
        }

        public override string PortraitPath => $"res://mods/Komeiji_Koishi/images/cards/{GetType().Name}.png";

        protected override IEnumerable<DynamicVar> CanonicalVars => new List<DynamicVar>
        {
            new PowerVar<JiasuiPower>(8m)
        };

        protected override IEnumerable<IHoverTip> ExtraHoverTips => new[]
        {
            HoverTipFactory.FromPower<JiasuiPower>()
        };

        protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
        {
            ArgumentNullException.ThrowIfNull(cardPlay.Target, nameof(cardPlay.Target));

            await PowerCmd.Apply<JiasuiPower>(
                choiceContext,
                cardPlay.Target,
                base.DynamicVars["JiasuiPower"].BaseValue,
                base.Owner.Creature,
                this,
                false
            );

            JiasuiPower? power = cardPlay.Target.GetPower<JiasuiPower>();
            if (power != null)
            {
                await power.Trigger(choiceContext, base.Owner.Creature, this);
            }
        }

        protected override void OnUpgrade()
        {
            base.DynamicVars["JiasuiPower"].UpgradeValueBy(3m);
        }
    }
}
