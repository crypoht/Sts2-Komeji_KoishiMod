using System.Collections.Generic;
using System.Threading.Tasks;
using BaseLib.Abstracts;
using BaseLib.Utils;
using KomeijiKoishi.Pools;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Relics;
using MegaCrit.Sts2.Core.Rooms;
using MegaCrit.Sts2.Core.ValueProps;

namespace KomeijiKoishi.Relics
{
    [Pool(typeof(KoishiSharedRelicPool))]
    public sealed class FadedMikoClothes_Koishi : CustomRelicModel
    {
        private const string DamageThresholdKey = "DamageThreshold";

        private int _damageTakenThisCombat;

        public override RelicRarity Rarity => RelicRarity.Ancient;

        public override bool ShowCounter => CombatManager.Instance.IsInProgress;

        public override int DisplayAmount => this.DamageTakenThisCombat;

        protected override IEnumerable<DynamicVar> CanonicalVars => new List<DynamicVar>
        {
            new EnergyVar(1),
            new DynamicVar(DamageThresholdKey, 10m)
        };

        protected override IEnumerable<IHoverTip> ExtraHoverTips => new[]
        {
            HoverTipFactory.ForEnergy(this)
        };

        public override string PackedIconPath => "res://mods/Komeiji_Koishi/images/relics/FadedMikoClothes_Koishi.png";
        protected override string PackedIconOutlinePath => "res://mods/Komeiji_Koishi/images/relics/FadedMikoClothes_Koishi.png";
        protected override string BigIconPath => "res://mods/Komeiji_Koishi/images/relics/FadedMikoClothes_Koishi.png";

        private int DamageTakenThisCombat
        {
            get => this._damageTakenThisCombat;
            set
            {
                base.AssertMutable();
                this._damageTakenThisCombat = value;
                this.RefreshStatus();
            }
        }

        private bool IsDisabled => this.DamageTakenThisCombat >= base.DynamicVars[DamageThresholdKey].IntValue;

        public override decimal ModifyMaxEnergy(Player player, decimal amount)
        {
            if (player != base.Owner || this.IsDisabled)
            {
                return amount;
            }

            return amount + base.DynamicVars.Energy.BaseValue;
        }

        public override Task AfterDamageReceived(
            PlayerChoiceContext choiceContext,
            Creature target,
            DamageResult result,
            ValueProp props,
            Creature? dealer,
            CardModel? cardSource
        )
        {
            if (!CombatManager.Instance.IsInProgress || target != base.Owner.Creature || result.UnblockedDamage <= 0)
            {
                return Task.CompletedTask;
            }

            this.DamageTakenThisCombat++;
            base.Flash();
            return Task.CompletedTask;
        }

        public override Task AfterCombatEnd(CombatRoom _)
        {
            this.DamageTakenThisCombat = 0;
            base.Status = RelicStatus.Normal;
            base.InvokeDisplayAmountChanged();
            return Task.CompletedTask;
        }

        private void RefreshStatus()
        {
            base.Status = this.IsDisabled ? RelicStatus.Disabled : RelicStatus.Active;
            base.InvokeDisplayAmountChanged();
        }
    }
}
