using System.Collections.Generic;
using System.Threading.Tasks;
using BaseLib.Abstracts;
using BaseLib.Utils;
using KomeijiKoishi.Pools;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Relics;
using MegaCrit.Sts2.Core.Rooms;
using MegaCrit.Sts2.Core.ValueProps;

namespace KomeijiKoishi.Relics
{
    [Pool(typeof(KoishiSharedRelicPool))]
    public sealed class MikoBlessing_Koishi : CustomRelicModel
    {
        private const string DamageThresholdKey = "DamageThreshold";
        private const string ImmunitiesKey = "Immunities";

        private int _immunitiesUsedThisCombat;

        public override RelicRarity Rarity => RelicRarity.Ancient;

        public override bool ShowCounter => CombatManager.Instance.IsInProgress;

        public override int DisplayAmount => this.RemainingImmunities;

        protected override IEnumerable<DynamicVar> CanonicalVars => new List<DynamicVar>
        {
            new DynamicVar(ImmunitiesKey, 3m),
            new DynamicVar(DamageThresholdKey, 20m)
        };

        public override string PackedIconPath => "res://mods/Komeiji_Koishi/images/relics/MikoBlessing_Koishi.png";
        protected override string PackedIconOutlinePath => "res://mods/Komeiji_Koishi/images/relics/MikoBlessing_Koishi.png";
        protected override string BigIconPath => "res://mods/Komeiji_Koishi/images/relics/MikoBlessing_Koishi.png";

        private int RemainingImmunities => base.DynamicVars[ImmunitiesKey].IntValue - this._immunitiesUsedThisCombat;

        public override decimal ModifyHpLostAfterOstyLate(Creature? target, decimal amount, ValueProp props, Creature? dealer, CardModel? cardSource)
        {
            if (!CombatManager.Instance.IsInProgress || target != base.Owner.Creature)
            {
                return amount;
            }

            if (this.RemainingImmunities <= 0 || amount <= base.DynamicVars[DamageThresholdKey].BaseValue)
            {
                return amount;
            }

            return 0m;
        }

        public override Task AfterModifyingHpLostAfterOsty()
        {
            base.AssertMutable();
            this._immunitiesUsedThisCombat++;
            this.RefreshStatus();
            base.Flash();
            return Task.CompletedTask;
        }

        public override Task AfterCombatEnd(CombatRoom _)
        {
            base.AssertMutable();
            this._immunitiesUsedThisCombat = 0;
            base.Status = RelicStatus.Normal;
            base.InvokeDisplayAmountChanged();
            return Task.CompletedTask;
        }

        private void RefreshStatus()
        {
            base.Status = this.RemainingImmunities <= 0 ? RelicStatus.Disabled : RelicStatus.Normal;
            base.InvokeDisplayAmountChanged();
        }
    }
}
