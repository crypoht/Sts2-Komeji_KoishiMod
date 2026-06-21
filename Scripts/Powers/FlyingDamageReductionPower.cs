using System.Collections.Generic;
using System.Threading.Tasks;
using BaseLib.Abstracts;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.ValueProps;

namespace KomeijiKoishi.Powers
{
    public sealed class FlyingDamageReductionPower : CustomPowerModel
    {
        private const string DamageDecreaseKey = "DamageDecrease";

        public override PowerType Type => PowerType.Buff;

        public override PowerStackType StackType => PowerStackType.Counter;

        public override string? CustomPackedIconPath => "res://mods/Komeiji_Koishi/images/powers/FlyingDamageReductionPower.png";

        public override string? CustomBigIconPath => "res://mods/Komeiji_Koishi/images/powers/FlyingDamageReductionPower.png";

        protected override IEnumerable<DynamicVar> CanonicalVars => new List<DynamicVar>
        {
            new DynamicVar(DamageDecreaseKey, 0.5m)
        };

        public override decimal ModifyDamageMultiplicative(
            Creature? target,
            decimal amount,
            ValueProp props,
            Creature? dealer,
            CardModel? cardSource
        )
        {
            if (target != base.Owner)
            {
                return 1m;
            }

            if (!props.IsPoweredAttack())
            {
                return 1m;
            }

            return base.DynamicVars[DamageDecreaseKey].BaseValue;
        }

        public override async Task AfterSideTurnEnd(PlayerChoiceContext choiceContext, CombatSide side, IEnumerable<Creature> participants)
        {
            if (side == CombatSide.Enemy)
            {
                await PowerCmd.Remove(this);
            }
        }
    }
}
