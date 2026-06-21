using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BaseLib.Abstracts;
using BaseLib.Utils;
using KomeijiKoishi.Pools;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.Models.Relics;
using MegaCrit.Sts2.Core.Rooms;
using MegaCrit.Sts2.Core.Saves.Runs;

namespace KomeijiKoishi.Relics
{
    [Pool(typeof(KoishiSharedRelicPool))]
    public sealed class TinyWhiteSnake_Koishi : CustomRelicModel
    {
        private const string TurnsKey = "Turns";

        private int _turnsSeen;

        public override RelicRarity Rarity => RelicRarity.Ancient;

        public override bool ShowCounter => true;

        public override int DisplayAmount => base.DynamicVars[TurnsKey].IntValue - this.TurnsSeen;

        protected override IEnumerable<DynamicVar> CanonicalVars => new List<DynamicVar>
        {
            new PowerVar<StrengthPower>(1m),
            new DynamicVar(TurnsKey, 2m)
        };

        protected override IEnumerable<IHoverTip> ExtraHoverTips => new List<IHoverTip>
        {
            HoverTipFactory.FromPower<StrengthPower>()
        };

        public override string PackedIconPath => $"res://mods/Komeiji_Koishi/images/relics/TinyWhiteSnake_Koishi.png";
        protected override string PackedIconOutlinePath => $"res://mods/Komeiji_Koishi/images/relics/TinyWhiteSnake_Koishi.png";
        protected override string BigIconPath => $"res://mods/Komeiji_Koishi/images/relics/TinyWhiteSnake_Koishi.png";

        [SavedProperty]
        public int TurnsSeen
        {
            get => this._turnsSeen;
            set
            {
                base.AssertMutable();
                this._turnsSeen = value;
                base.InvokeDisplayAmountChanged();
            }
        }

        public override async Task AfterSideTurnStart(CombatSide side, IReadOnlyList<Creature> participants, ICombatState combatState)
        {
            if (!participants.Contains(base.Owner.Creature))
            {
                return;
            }

            this.TurnsSeen = (this.TurnsSeen + 1) % base.DynamicVars[TurnsKey].IntValue;
            base.Status = this.TurnsSeen == base.DynamicVars[TurnsKey].IntValue - 1
                ? RelicStatus.Active
                : RelicStatus.Normal;

            if (this.TurnsSeen != 0)
            {
                return;
            }

            base.Flash();
            decimal strengthLoss = -base.DynamicVars["StrengthPower"].BaseValue;

            foreach (Creature enemy in combatState.HittableEnemies)
            {
                await PowerCmd.Apply<StrengthPower>(
                    new ThrowingPlayerChoiceContext(),
                    enemy,
                    strengthLoss,
                    base.Owner.Creature,
                    null,
                    false
                );
            }
        }

        public override Task AfterCombatEnd(CombatRoom _)
        {
            base.Status = RelicStatus.Normal;
            return Task.CompletedTask;
        }
    }
}
