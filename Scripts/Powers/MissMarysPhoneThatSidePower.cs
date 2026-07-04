using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BaseLib.Abstracts;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;

namespace KomeijiKoishi.Powers
{
    public sealed class MissMarysPhoneThatSidePower : CustomPowerModel
    {
        public override PowerType Type => PowerType.Buff;
        public override PowerStackType StackType => PowerStackType.None;

        public override string? CustomPackedIconPath => "res://mods/Komeiji_Koishi/images/powers/MissMarysPhoneThatSidePower.png";
        public override string? CustomBigIconPath => "res://mods/Komeiji_Koishi/images/powers/MissMarysPhoneThatSidePower.png";

        public override bool TryModifyEnergyCostInCombatLate(CardModel card, decimal originalCost, out decimal modifiedCost)
        {
            modifiedCost = originalCost;
            if (!ShouldMakeFree(card))
            {
                return false;
            }

            modifiedCost = 0m;
            return true;
        }

        public override bool TryModifyStarCost(CardModel card, decimal originalCost, out decimal modifiedCost)
        {
            modifiedCost = originalCost;
            if (!ShouldMakeFree(card))
            {
                return false;
            }

            modifiedCost = 0m;
            return true;
        }

        public override async Task BeforeSideTurnEnd(PlayerChoiceContext choiceContext, CombatSide side, IEnumerable<Creature> participants)
        {
            if (participants.Contains(base.Owner))
            {
                await PowerCmd.Remove(this);
            }
        }

        private bool ShouldMakeFree(CardModel card)
        {
            if (card.Owner?.Creature != base.Owner || card.Type != CardType.Attack)
            {
                return false;
            }

            var pileType = card.Pile?.Type;
            return pileType == PileType.Hand || pileType == PileType.Play;
        }
    }
}
