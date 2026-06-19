using System.Collections.Generic;
using System.Threading.Tasks;
using BaseLib.Abstracts;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips; 
using MegaCrit.Sts2.Core.MonsterMoves.Intents; 
using KomeijiKoishi.Pools;
using MegaCrit.Sts2.Core.Models.CardPools;
using BaseLib.Utils;

namespace KomeijiKoishi.Cards.Fumo
{
    [Pool(typeof(TokenCardPool))]
    public sealed class KogasaFumo_Koishi : CustomCardModel
    {

        public KogasaFumo_Koishi() 
            : base(0, CardType.Skill, CardRarity.Token, TargetType.AnyEnemy, true) { }

        public override string PortraitPath => $"res://mods/Komeiji_Koishi/images/cards/fumo/{GetType().Name}.png";
        public override IEnumerable<CardKeyword> CanonicalKeywords => new[] { CardKeyword.Exhaust };

        protected override IEnumerable<IHoverTip> ExtraHoverTips => new List<IHoverTip> 
        { 
            StunIntent.GetStaticHoverTip() 
        };

        protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
        {
            if (cardPlay.Target == null) return;

            await CreatureCmd.Stun(cardPlay.Target, null);
        }

        protected override void OnUpgrade()
        {
            base.EnergyCost.UpgradeBy(+385); 
        }
    }
}