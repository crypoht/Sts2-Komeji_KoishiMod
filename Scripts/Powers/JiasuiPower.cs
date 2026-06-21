using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BaseLib.Abstracts;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.ValueProps;

namespace KomeijiKoishi.Powers
{
    public sealed class JiasuiPower : CustomPowerModel
    {
        public override PowerType Type => PowerType.Debuff;

        public override PowerStackType StackType => PowerStackType.Counter;

        public override string? CustomPackedIconPath => "res://mods/Komeiji_Koishi/images/powers/JiasuiPower.png";

        public override string? CustomBigIconPath => "res://mods/Komeiji_Koishi/images/powers/JiasuiPower.png";

        public async Task Trigger(PlayerChoiceContext choiceContext, Creature applier, CardModel? sourceCard)
        {
            int amount = base.Amount;
            if (amount <= 0)
            {
                return;
            }

            base.Flash();
            await CreatureCmd.Damage(choiceContext, base.Owner, amount, ValueProp.Unblockable | ValueProp.Unpowered, null, null);

            if (base.Owner.CombatState == null)
            {
                return;
            }

            IReadOnlyList<Creature> otherEnemies = base.Owner.CombatState.HittableEnemies
                .Where(enemy => enemy != base.Owner && !enemy.IsDead)
                .ToList();

            foreach (Creature enemy in otherEnemies)
            {
                await PowerCmd.Apply<JiasuiPower>(choiceContext, enemy, amount, applier, sourceCard, false);
            }
        }
    }
}
