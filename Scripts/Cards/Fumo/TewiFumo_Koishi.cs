using System.Collections.Generic;
using System.Threading.Tasks;
using BaseLib.Abstracts;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Cards;
using MegaCrit.Sts2.Core.Models.Powers;
using KomeijiKoishi.Pools;
using MegaCrit.Sts2.Core.Models.CardPools;
using BaseLib.Utils;
using MegaCrit.Sts2.Core.HoverTips;

namespace KomeijiKoishi.Cards.Fumo
{
    [Pool(typeof(TokenCardPool))]
    public sealed class TewiFumo_Koishi : CustomCardModel
    {
        public TewiFumo_Koishi() 
            : base(0, CardType.Skill, CardRarity.Token, TargetType.Self, true) { }

        public override string PortraitPath => $"res://mods/Komeiji_Koishi/images/cards/fumo/{GetType().Name}.png";
        public override IEnumerable<CardKeyword> CanonicalKeywords => new[] { CardKeyword.Exhaust };

        protected override IEnumerable<IHoverTip> ExtraHoverTips => new List<IHoverTip> 
        { 
            HoverTipFactory.FromCard<ReisenFumo_Koishi>(false)
        };

        protected override IEnumerable<DynamicVar> CanonicalVars => new List<DynamicVar> 
        { 
            new PowerVar<RollingBoulderPower>(5m) 
        };

        protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
        {
            var player = base.Owner as Player;
            if (player == null || base.CombatState == null) return;

            await PowerCmd.Apply<RollingBoulderPower>(
                choiceContext,
                player.Creature, 
                base.DynamicVars["RollingBoulderPower"].BaseValue, 
                player.Creature, 
                this, 
                false
            );

            for (int i = 0; i < 10; i++)
            {

                CardModel reisenFumo = base.CombatState.CreateCard<ReisenFumo_Koishi>(player);
                
                await CardPileCmd.AddGeneratedCardToCombat(reisenFumo, PileType.Hand, player, CardPilePosition.Bottom);
            }
        }

        protected override void OnUpgrade()
        {
            base.EnergyCost.UpgradeBy(+385); 
        }
    }
}