using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BaseLib.Abstracts;
using BaseLib.Utils;
using KomeijiKoishi.Pools;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.Relics;
using MegaCrit.Sts2.Core.Rooms;

namespace KomeijiKoishi.Relics
{
    [Pool(typeof(KoishiSharedRelicPool))]
    public sealed class FrogClothes_Koishi : CustomRelicModel
    {
        private decimal _currentHealAmount;

        public override RelicRarity Rarity => RelicRarity.Ancient;

        public override bool ShowCounter => CombatManager.Instance.IsInProgress;

        public override int DisplayAmount => (int)this.CurrentHealAmount;

        protected override IEnumerable<DynamicVar> CanonicalVars => new List<DynamicVar>
        {
            new HealVar(12m),
            new DynamicVar("Reduction", 3m)
        };

        public override string PackedIconPath => $"res://mods/Komeiji_Koishi/images/relics/FrogClothes_Koishi.png";
        protected override string PackedIconOutlinePath => $"res://mods/Komeiji_Koishi/images/relics/FrogClothes_Koishi.png";
        protected override string BigIconPath => $"res://mods/Komeiji_Koishi/images/relics/FrogClothes_Koishi.png";

        public decimal CurrentHealAmount
        {
            get => this._currentHealAmount;
            set
            {
                base.AssertMutable();
                this._currentHealAmount = value;
                this.RefreshCounter();
            }
        }

        public override Task BeforeCombatStart()
        {
            this.ResetHealAmount();
            return Task.CompletedTask;
        }

        public override Task AfterSideTurnStart(CombatSide side, IReadOnlyList<Creature> participants, ICombatState combatState)
        {
            if (participants.Contains(base.Owner.Creature))
            {
                this.ResetHealAmount();
            }

            return Task.CompletedTask;
        }

        public override Task AfterCardPlayed(PlayerChoiceContext choiceContext, CardPlay cardPlay)
        {
            if (cardPlay.Card.Owner == base.Owner && this.CurrentHealAmount > 0m)
            {
                this.CurrentHealAmount = this.CurrentHealAmount - base.DynamicVars["Reduction"].BaseValue;
                if (this.CurrentHealAmount < 0m)
                {
                    this.CurrentHealAmount = 0m;
                }
            }

            return Task.CompletedTask;
        }

        public override async Task BeforeSideTurnEnd(PlayerChoiceContext choiceContext, CombatSide side, IEnumerable<Creature> participants)
        {
            if (!participants.Contains(base.Owner.Creature))
            {
                return;
            }

            decimal healAmount = this.CurrentHealAmount;
            if (healAmount > 0m)
            {
                base.Flash();
                await CreatureCmd.Heal(base.Owner.Creature, healAmount, true);
            }

            this.CurrentHealAmount = 0m;
        }

        public override Task AfterCombatEnd(CombatRoom _)
        {
            this.CurrentHealAmount = 0m;
            base.Status = RelicStatus.Normal;
            base.InvokeDisplayAmountChanged();
            return Task.CompletedTask;
        }

        private void ResetHealAmount()
        {
            this.CurrentHealAmount = base.DynamicVars.Heal.BaseValue;
        }

        private void RefreshCounter()
        {
            base.Status = this.CurrentHealAmount > 0m ? RelicStatus.Active : RelicStatus.Normal;
            base.InvokeDisplayAmountChanged();
        }
    }
}
