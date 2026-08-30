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
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.Saves.Runs;

namespace KomeijiKoishi.Powers
{
    public sealed class MissMarysPhoneThisSidePower : CustomPowerModel
    {
        public override PowerType Type => PowerType.Buff;
        public override PowerStackType StackType => PowerStackType.Counter;

        public override string? CustomPackedIconPath => "res://mods/Komeiji_Koishi/images/powers/MissMarysPhoneThisSidePower.png";
        public override string? CustomBigIconPath => "res://mods/Komeiji_Koishi/images/powers/MissMarysPhoneThisSidePower.png";

        [SavedProperty]
        public int AttackThreshold { get; private set; }

        public override Task BeforeApplied(
            Creature target,
            decimal amount,
            Creature? applier,
            CardModel? cardSource)
        {
            // Amount is also used as the visible remaining counter. Keep the
            // original threshold separately so the counter can be reset.
            if (AttackThreshold <= 0)
            {
                AttackThreshold = (int)amount;
            }

            return Task.CompletedTask;
        }

        public override async Task AfterCardPlayed(PlayerChoiceContext choiceContext, CardPlay cardPlay)
        {
            if (cardPlay.Card.Type != CardType.Attack || !IsAffectedAlly(cardPlay.Card.Owner?.Creature))
            {
                return;
            }

            int threshold = GetAttackThreshold();
            if (threshold <= 0)
            {
                return;
            }

            // Store the remaining attacks in Amount so the counter is visible
            // and synchronized by the game's normal power state replication.
            int remaining = (int)base.Amount;
            if (remaining <= 0)
            {
                remaining = threshold;
            }

            if (remaining > 1)
            {
                await PowerCmd.ModifyAmount(
                    choiceContext,
                    this,
                    -1m,
                    base.Owner,
                    null,
                    false);
                return;
            }

            base.Flash();
            await PowerCmd.Apply<IntangiblePower>(choiceContext, base.Owner, 1m, base.Owner, null, false);

            // The triggering attack consumed the final count. Reset directly
            // to the next threshold without briefly removing the power.
            await PowerCmd.ModifyAmount(
                choiceContext,
                this,
                threshold - remaining,
                base.Owner,
                null,
                false);
        }

        public override async Task BeforeSideTurnEnd(PlayerChoiceContext choiceContext, CombatSide side, IEnumerable<Creature> participants)
        {
            if (participants.Contains(base.Owner))
            {
                await PowerCmd.Remove(this);
            }
        }

        private bool IsAffectedAlly(Creature? creature)
        {
            return creature != null && creature != base.Owner && creature.Side == base.Owner.Side && !creature.IsDead;
        }

        private int GetAttackThreshold()
        {
            return AttackThreshold > 0 ? AttackThreshold : (int)base.Amount;
        }
    }
}
