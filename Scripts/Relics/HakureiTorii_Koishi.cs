using System.Collections.Generic;
using System.Threading.Tasks;
using BaseLib.Abstracts;
using BaseLib.Utils;
using KomeijiKoishi.Pools;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Relics;
using MegaCrit.Sts2.Core.ValueProps;

namespace KomeijiKoishi.Relics
{
    [Pool(typeof(KoishiSharedRelicPool))]
    public sealed class HakureiTorii_Koishi : CustomRelicModel
    {
        private const string DamageThresholdKey = "DamageThreshold";

        public override RelicRarity Rarity => RelicRarity.Ancient;

        protected override IEnumerable<DynamicVar> CanonicalVars => new List<DynamicVar>
        {
            new DynamicVar(DamageThresholdKey, 5m),
            new HealVar(1m)
        };

        public override string PackedIconPath => "res://mods/Komeiji_Koishi/images/relics/HakureiTorii_Koishi.png";
        protected override string PackedIconOutlinePath => "res://mods/Komeiji_Koishi/images/relics/HakureiTorii_Koishi.png";
        protected override string BigIconPath => "res://mods/Komeiji_Koishi/images/relics/HakureiTorii_Koishi.png";

        public override decimal ModifyHpLostAfterOstyLate(Creature? target, decimal amount, ValueProp props, Creature? dealer, CardModel? cardSource)
        {
            if (target != base.Owner.Creature)
            {
                return amount;
            }

            if (amount <= 0m || amount >= base.DynamicVars[DamageThresholdKey].BaseValue)
            {
                return amount;
            }

            return 0m;
        }

        public override async Task AfterModifyingHpLostAfterOsty()
        {
            base.Flash();
            await CreatureCmd.Heal(base.Owner.Creature, base.DynamicVars.Heal.BaseValue, true);
        }
    }
}
