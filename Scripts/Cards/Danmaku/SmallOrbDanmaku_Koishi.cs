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
using MegaCrit.Sts2.Core.Models.CardPools;

namespace KomeijiKoishi.Cards.Danmaku
{
    [Pool(typeof(TokenCardPool))]
    public sealed class SmallOrbDanmaku_Koishi : CustomCardModel
    {
        public SmallOrbDanmaku_Koishi() 
            : base(0, CardType.Attack, CardRarity.Token, TargetType.AnyEnemy, true) 
        { 
        }

        public override string PortraitPath => KoishiImagePaths.CardPortrait(GetType());

        public override IEnumerable<CardKeyword> CanonicalKeywords => new[] { CardKeyword.Exhaust };

        protected override HashSet<CardTag> CanonicalTags => new HashSet<CardTag> 
        { 
            KoishiTags.Danmaku 
        };

        protected override IEnumerable<DynamicVar> CanonicalVars => new List<DynamicVar> 
        { 
            new DamageVar(1m, ValueProp.Move),
            new RepeatVar(3) 
        };

        protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
        {
            if (cardPlay.Target == null) return;

            for (int i = 0; i < base.DynamicVars.Repeat.IntValue; i++)
            {
                DanmakuProjectileHelper.AddToCombat(base.Owner.Creature, cardPlay.Target, "smallorb");
                await Cmd.Wait(0.04f, false);
            }

            await DamageCmd.Attack(base.DynamicVars.Damage.BaseValue)
                .WithHitCount(base.DynamicVars.Repeat.IntValue) 
                .FromCard(this)
                .Targeting(cardPlay.Target)
                .Execute(choiceContext);
        }

        protected override void OnUpgrade()
        {
            base.DynamicVars.Damage.UpgradeValueBy(1m); 
        }
    }
}
