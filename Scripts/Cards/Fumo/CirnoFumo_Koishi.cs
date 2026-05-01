using System.Collections.Generic;
using System.Threading.Tasks;
using BaseLib.Abstracts;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.Cards;
using MegaCrit.Sts2.Core.HoverTips; 
using MegaCrit.Sts2.Core.Models.Powers; 
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using KomeijiKoishi.Pools;
using MegaCrit.Sts2.Core.Models.CardPools;
using BaseLib.Utils;

namespace KomeijiKoishi.Cards.Fumo
{
    [Pool(typeof(TokenCardPool))]
    public sealed class CirnoFumo_Koishi : CustomCardModel
    {
        public CirnoFumo_Koishi() 
            : base(0, CardType.Skill, CardRarity.Token, TargetType.Self, true) { }

        public override string PortraitPath => $"res://mods/Komeiji_Koishi/images/cards/fumo/{GetType().Name}.png";
        public override IEnumerable<CardKeyword> CanonicalKeywords => new[] { CardKeyword.Exhaust };


        protected override IEnumerable<IHoverTip> ExtraHoverTips => new List<IHoverTip> 
        { 
            HoverTipFactory.FromPower<PlatingPower>()
        };

        protected override IEnumerable<DynamicVar> CanonicalVars => new List<DynamicVar> 
        { 
            new PowerVar<PlatingPower>(9m) 
        };

        protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
        {
            var player = base.Owner as Player;
            if (player == null) return;

            await PowerCmd.Apply<PlatingPower>(
                player.Creature, 
                base.DynamicVars["PlatingPower"].BaseValue, 
                player.Creature, 
                this, 
                false
            );
        }

        protected override void OnUpgrade()
        {
            base.EnergyCost.UpgradeBy(+385); 
        }
    }
}