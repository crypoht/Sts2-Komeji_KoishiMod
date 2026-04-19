using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BaseLib.Abstracts;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Models.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using KomeijiKoishi.Pools;
using KomeijiKoishi.Powers;
using MegaCrit.Sts2.Core.Models.CardPools;
using KomeijiKoishi.Enums;
using BaseLib.Utils;  
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models; 
using MegaCrit.Sts2.Core.Models.Powers; 
using MegaCrit.Sts2.Core.ValueProps;

namespace KomeijiKoishi.Cards.Danmaku
{
    [Pool(typeof(TokenCardPool))]
    public sealed class ConsciousnessRotation_Koishi : CustomCardModel
    {
        public ConsciousnessRotation_Koishi() 
            : base(0, CardType.Skill, CardRarity.Token, TargetType.Self, true) 
        { 
        }

        public override string PortraitPath => $"res://mods/Komeiji_Koishi/images/cards/{GetType().Name}.png";

        public override IEnumerable<CardKeyword> CanonicalKeywords => new[] { CardKeyword.Ethereal };

        protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
        {
            var player = base.Owner as MegaCrit.Sts2.Core.Entities.Players.Player;
            if (player == null) return;

            bool isClosed = player.Creature.Powers.Any(p => p is ClosedStancePower);
            bool isBloom = player.Creature.Powers.Any(p => p is BloomStancePower);

            if (isClosed)
            {
                await BloomStancePower.EnterThisStance(choiceContext, player, this);
            }
            else
            {
                await ClosedStancePower.EnterThisStance(choiceContext, player, this);
            }

            base.EnergyCost.AddThisCombat(1, false);
        }

        protected override PileType GetResultPileType()
        {
            PileType resultPileType = base.GetResultPileType();
            
            if (resultPileType == PileType.Discard)
            {
                return PileType.Hand;
            }
            
            return resultPileType;
        }

        protected override void OnUpgrade()
        {
            base.EnergyCost.UpgradeBy(-1);
        }
    }
}