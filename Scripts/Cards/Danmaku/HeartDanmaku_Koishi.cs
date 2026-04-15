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
    public sealed class HeartDanmaku_Koishi : CustomCardModel
    {
        public HeartDanmaku_Koishi() 
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
            new DamageVar(4m, ValueProp.Move), 
            new PowerVar<WeakPower>(1m)        
        };

        protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
        {
            if (cardPlay.Target == null) return;

            await DamageCmd.Attack(base.DynamicVars.Damage.BaseValue)
                .FromCard(this)
                .Targeting(cardPlay.Target)
                .Execute(choiceContext);

            await PowerCmd.Apply<WeakPower>(
                cardPlay.Target, 
                base.DynamicVars["WeakPower"].BaseValue, 
                base.Owner.Creature, 
                this, 
                false
            );
        }

        protected override void OnUpgrade()
        {
            base.DynamicVars.Damage.UpgradeValueBy(1m); // 4(5)
        }
    }
}