using System.Collections.Generic;
using System.Threading.Tasks;
using BaseLib.Abstracts;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;

namespace KomeijiKoishi.Powers
{
    public sealed class GuardianPower : CustomPowerModel
    {
        public override PowerType Type => PowerType.Buff;

        public override PowerStackType StackType => PowerStackType.Counter;

        public override string? CustomPackedIconPath => "res://mods/Komeiji_Koishi/images/powers/GuardianPower.png";

        public override string? CustomBigIconPath => "res://mods/Komeiji_Koishi/images/powers/GuardianPower.png";

        public override bool TryModifyPowerAmountReceived(
            PowerModel canonicalPower,
            Creature target,
            decimal amount,
            Creature? applier,
            out decimal modifiedAmount
        )
        {
            if (target != base.Owner || canonicalPower.GetTypeForAmount(amount) != PowerType.Debuff || !canonicalPower.IsVisible)
            {
                modifiedAmount = amount;
                return false;
            }

            modifiedAmount = 0m;
            return true;
        }

        public override Task AfterModifyingPowerAmountReceived(PowerModel power)
        {
            base.Flash();
            return Task.CompletedTask;
        }

        public override async Task AfterSideTurnEnd(PlayerChoiceContext choiceContext, CombatSide side, IEnumerable<Creature> participants)
        {
            if (side == CombatSide.Enemy)
            {
                await PowerCmd.Decrement(this);
            }
        }

        public override decimal GetScaledAmountForMultiplayer(
            ICombatState combatState,
            Creature? applier,
            decimal amount,
            Creature target,
            CardModel? cardSource
        )
        {
            return amount + combatState.Players.Count - 1m;
        }
    }
}
