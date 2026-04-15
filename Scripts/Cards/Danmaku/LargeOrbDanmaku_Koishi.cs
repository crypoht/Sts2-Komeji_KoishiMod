using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using BaseLib.Utils; 
using BaseLib.Abstracts; 
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Cards;
using MegaCrit.Sts2.Core.Models.Powers; 
using MegaCrit.Sts2.Core.ValueProps;
using MegaCrit.Sts2.Core.GameActions.Multiplayer; 
using KomeijiKoishi.Pools; 
using KomeijiKoishi.Enums;

namespace KomeijiKoishi.Cards.Danmaku
{
    [Pool(typeof(DanmakuCardPool))]
    public sealed class LargeOrbDanmaku_Koishi : CustomCardModel
    {
        public LargeOrbDanmaku_Koishi() 
            : base(0, CardType.Attack, CardRarity.Common, TargetType.AnyEnemy, false) 
        { 
        }

        public override string PortraitPath => $"res://mods/Komeiji_Koishi/images/cards/{GetType().Name}.png";

        public override IEnumerable<CardKeyword> CanonicalKeywords => new[] { CardKeyword.Exhaust };

        protected override HashSet<CardTag> CanonicalTags => new HashSet<CardTag> 
        { 
            KoishiTags.Danmaku 
        };

        protected override IEnumerable<DynamicVar> CanonicalVars => new List<DynamicVar> 
        { 
            new DamageVar(8m, ValueProp.Move),
            new PowerVar<VulnerablePower>(1m) 
        };

        protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
        {
            if (cardPlay.Target == null) return;

            await DamageCmd.Attack(base.DynamicVars.Damage.BaseValue)
                .FromCard(this)
                .Targeting(cardPlay.Target)
                .Execute(choiceContext);

            await PowerCmd.Apply<VulnerablePower>(
                cardPlay.Target, 
                base.DynamicVars["VulnerablePower"].BaseValue, 
                base.Owner.Creature, 
                this, 
                false
            );
        }

        protected override void OnUpgrade()
        {
            base.DynamicVars["VulnerablePower"].UpgradeValueBy(1m); // 易伤 1(2)
        }
    }
}