using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models.Powers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BaseLib.Utils;
using BaseLib.Abstracts; 
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.Entities.Creatures; 
using MegaCrit.Sts2.Core.Models;
using KomeijiKoishi.Enums;


namespace KomeijiKoishi.Powers
{
    public sealed class JoyMaskPower : CustomPowerModel
    {
        public override PowerType Type => PowerType.Buff;
        public override PowerStackType StackType => PowerStackType.Counter;

        public override string? CustomPackedIconPath => "res://mods/Komeiji_Koishi/images/powers/JoyMaskPower.png";

        public override string? CustomBigIconPath => "res://mods/Komeiji_Koishi/images/powers/JoyMaskPower.png";

        private CardModel? _ignoredCard; 

        public override Task BeforeApplied(Creature target, decimal amount, Creature? applier, CardModel? cardSource)
        {
            if (cardSource != null)
            {
                _ignoredCard = cardSource;
            }
            return Task.CompletedTask;
        }

        public override bool TryModifyEnergyCostInCombat(CardModel card, decimal originalCost, out decimal modifiedCost)
        {
            modifiedCost = 0m;
            return true;
        }

        public override async Task AfterCardPlayed(PlayerChoiceContext context, CardPlay cardPlay)
        {
            if (_ignoredCard != null && cardPlay.Card == _ignoredCard)
            {
                _ignoredCard = null; 
                return; 
            }

            if (cardPlay.Card.Owner.Creature == base.Owner && !cardPlay.IsAutoPlay)
            {
                await PowerCmd.Apply<JoyMaskPower>(base.Owner, -1m, base.Owner, null, false);
            }
        }
    }
}