using System.Collections.Generic;
using System.Threading.Tasks;
using BaseLib.Abstracts;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.Relics;
using MegaCrit.Sts2.Core.ValueProps;
using KomeijiKoishi.Pools;
using KomeijiKoishi.Powers; 
using BaseLib.Utils;
using MegaCrit.Sts2.Core.Entities.Relics;

namespace KomeijiKoishi.Relics
{
    [Pool(typeof(KoishiRelicPool))]
    public sealed class RoseBouquet_Koishi : CustomRelicModel
    {
        public override RelicRarity Rarity => RelicRarity.Common;

        protected override IEnumerable<DynamicVar> CanonicalVars => new List<DynamicVar> 
        { 
            new DamageVar(2m, ValueProp.Unpowered) 
        };



        public override string PackedIconPath => $"res://mods/Komeiji_Koishi/images/relics/RoseBouquet_Koishi_outline.png";
        protected override string PackedIconOutlinePath => $"res://mods/Komeiji_Koishi/images/relics/RoseBouquet_Koishi.png";
        protected override string BigIconPath => $"res://mods/Komeiji_Koishi/images/relics/RoseBouquet_Koishi.png";


        public override async Task BeforeCombatStart()
        {
            base.Flash(); 

  
            await PowerCmd.Apply<KuugaPower>(
                base.Owner.Creature,  
                base.DynamicVars.Damage.BaseValue,  
                base.Owner.Creature,  
                null,                 
                false                 
            );
        }
    }
}