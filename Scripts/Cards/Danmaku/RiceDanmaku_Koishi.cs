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
using MegaCrit.Sts2.Core.Nodes.CommonUi; 
using MegaCrit.Sts2.Core.GameActions.Multiplayer; 
using KomeijiKoishi.Pools; 
using KomeijiKoishi.Enums;

namespace KomeijiKoishi.Cards.Danmaku
{
    [Pool(typeof(DanmakuCardPool))]
    public sealed class RiceDanmaku_Koishi : CustomCardModel
    {
        public RiceDanmaku_Koishi() 
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
            new DamageVar(4m, ValueProp.Move)
        };

        protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
        {
            if (cardPlay.Target == null) return;

            await DamageCmd.Attack(base.DynamicVars.Damage.BaseValue)
                .FromCard(this)
                .Targeting(cardPlay.Target)
                .Execute(choiceContext);

            // 复制自身塞入弃牌堆
            CardModel cardModel = base.CreateClone();
            CardPileAddResult cardPileAddResult = await CardPileCmd.AddGeneratedCardToCombat(cardModel, PileType.Discard, true, CardPilePosition.Bottom);
            CardCmd.PreviewCardPileAdd(cardPileAddResult, 2.2f, CardPreviewStyle.HorizontalLayout);
        }

        protected override void OnUpgrade()
        {
            base.DynamicVars.Damage.UpgradeValueBy(1m); // 4(5)
        }
    }
}