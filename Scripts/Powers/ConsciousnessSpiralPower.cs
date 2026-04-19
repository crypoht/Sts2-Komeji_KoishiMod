using System.Threading.Tasks;
using BaseLib.Abstracts;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.Models.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using KomeijiKoishi.Cards.Danmaku;
using System;
using System.Collections.Generic;
using System.Linq;
using BaseLib.Utils;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.Entities.Creatures; 
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Localization;
using KomeijiKoishi.Enums; 
using MegaCrit.Sts2.Core.Entities.Players; 
using MegaCrit.Sts2.Core.Combat;

namespace KomeijiKoishi.Powers
{
    public sealed class ConsciousnessSpiralPower : CustomPowerModel
    {
        public override PowerType Type => PowerType.Buff;
        public override PowerStackType StackType => PowerStackType.Counter;

        public override string? CustomPackedIconPath => $"res://mods/Komeiji_Koishi/images/powers/ConsciousnessSpiralPower.png";

        public override async Task BeforeHandDraw(Player player, PlayerChoiceContext choiceContext, CombatState combatState)
        {
            if (player == base.Owner?.Player)
            {
                this.Flash();
                
                int count = (int)base.Amount;
                var cards = new List<CardModel>();
                
                for (int i = 0; i < count; i++)
                {
                    cards.Add(combatState.CreateCard<ConsciousnessRotation_Koishi>(player));
                }


                await CardPileCmd.AddGeneratedCardsToCombat(cards, PileType.Hand, true, CardPilePosition.Bottom);
            }
        }
    }
}