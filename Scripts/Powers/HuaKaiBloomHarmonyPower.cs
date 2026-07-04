using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BaseLib.Abstracts;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Cards;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.ValueProps;

namespace KomeijiKoishi.Powers
{
    public sealed class HuaKaiBloomHarmonyPower : CustomPowerModel
    {
        private CardModel? _sourceCard;

        public override PowerType Type => PowerType.Buff;
        public override PowerStackType StackType => PowerStackType.None;

        public override string? CustomPackedIconPath => "res://mods/Komeiji_Koishi/images/powers/PerfectMindControl.png";
        public override string? CustomBigIconPath => "res://mods/Komeiji_Koishi/images/powers/PerfectMindControl.png";

        public void BindSourceCard(CardModel sourceCard)
        {
            _sourceCard = sourceCard;
        }

        public override decimal ModifyBlockMultiplicative(
            Creature target,
            decimal block,
            ValueProp props,
            CardModel? cardSource,
            CardPlay? cardPlay)
        {
            if (target != base.Owner)
            {
                return 1m;
            }

            if (base.Owner.GetPower<BloomStancePower>() == null)
            {
                return 1m;
            }

            return 2.5m;
        }

        public override async Task AfterSideTurnStart(
            CombatSide side,
            IReadOnlyList<Creature> participants,
            ICombatState combatState)
        {
            if (side != base.Owner.Side)
            {
                return;
            }

            var player = base.Owner.Player;
            if (player == null)
            {
                await PowerCmd.Remove(this);
                return;
            }

            if (_sourceCard != null)
            {
                var bloomPower = base.Owner.GetPower<BloomStancePower>();
                if (bloomPower != null)
                {
                    await PowerCmd.Remove(bloomPower);
                }

                await ClosedStancePower.EnterThisStance(new ThrowingPlayerChoiceContext(), player, _sourceCard);
            }
            await PowerCmd.Remove(this);
        }
    }
}
