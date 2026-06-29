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
using MegaCrit.Sts2.Core.ValueProps;
using MegaCrit.Sts2.Core.GameActions.Multiplayer; 
using KomeijiKoishi.Pools; 
using KomeijiKoishi.Enums;
using MegaCrit.Sts2.Core.Models.CardPools;
using MegaCrit.Sts2.Core.HoverTips;

namespace KomeijiKoishi.Cards.Danmaku
{
    [Pool(typeof(TokenCardPool))]
    public sealed class YinYangOrbDanmaku_Koishi : CustomCardModel
    {
        public YinYangOrbDanmaku_Koishi() 
            : base(3, CardType.Attack, CardRarity.Token, TargetType.AnyEnemy, true) 
        { 
        }

        public override string PortraitPath => KoishiImagePaths.CardPortrait(GetType());
        protected override HashSet<CardTag> CanonicalTags => new HashSet<CardTag> 
        { 
            KoishiTags.Danmaku 
        };

        protected override IEnumerable<DynamicVar> CanonicalVars => new List<DynamicVar> 
        { 
            new DamageVar(20m, ValueProp.Move)
        };

        protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
        {
            if (cardPlay.Target == null) return;

            DanmakuProjectileHelper.AddToCombat(base.Owner.Creature, cardPlay.Target, "yinyangorb");

            await DamageCmd.Attack(base.DynamicVars.Damage.BaseValue)
                .FromCard(this)
                .Targeting(cardPlay.Target)
                .Execute(choiceContext);
        }

        protected override void OnUpgrade()
        {
            base.DynamicVars.Damage.UpgradeValueBy(9m);
        }
    }
}
