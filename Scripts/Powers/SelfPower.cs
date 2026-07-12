using System.Collections.Generic;
using System.Threading.Tasks;
using BaseLib.Abstracts;
using BaseLib.Utils;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;

namespace KomeijiKoishi.Powers
{
    public sealed class SelfPower : CustomPowerModel
    {
        public override PowerType Type => PowerType.Debuff;
        public override PowerStackType StackType => PowerStackType.Counter;
        public override string? CustomPackedIconPath => "res://mods/Komeiji_Koishi/images/powers/SelfPower.png";
        public override string? CustomBigIconPath => "res://mods/Komeiji_Koishi/images/powers/SelfPower.png";

        public override async Task AfterSideTurnStart(CombatSide side, IReadOnlyList<Creature> participants, ICombatState combatState)
        {
            if (side != base.Owner.Side)
            {
                base.Flash();
                await PowerCmd.Apply<KuugaPower>(new ThrowingPlayerChoiceContext(), base.Owner, -base.Amount, base.Owner, null, false);
                await PowerCmd.Remove(this);
            }

            await base.AfterSideTurnStart(side, participants, combatState);
        }
    }
}
