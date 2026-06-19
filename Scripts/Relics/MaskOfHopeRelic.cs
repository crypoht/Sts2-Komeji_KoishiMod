using System.Collections.Generic;
using System.Threading.Tasks;
using BaseLib.Abstracts;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.HoverTips;
using KomeijiKoishi.Powers; 
using MegaCrit.Sts2.Core.Entities.Relics; 
using MegaCrit.Sts2.Core.Models.Relics;   
using BaseLib.Utils;
using KomeijiKoishi.Pools;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;

namespace KomeijiKoishi.Relics
{

    [Pool(typeof(KoishiSharedRelicPool))]
    public sealed class MaskOfHopeRelic : CustomRelicModel
    {

        public override RelicRarity Rarity => RelicRarity.Uncommon;


        public override string PackedIconPath => $"res://mods/Komeiji_Koishi/images/relics/MaskOfHopeRelic.png";
        protected override string PackedIconOutlinePath => $"res://mods/Komeiji_Koishi/images/relics/MaskOfHopeRelic.png";
        protected override string BigIconPath => $"res://mods/Komeiji_Koishi/images/relics/MaskOfHopeRelic.png";


        public override async Task BeforeCombatStart()
        {
            base.Flash();

            await PowerCmd.Apply<JoyMaskPower>(
                new ThrowingPlayerChoiceContext(),
                base.Owner.Creature,  
                1m,                  
                base.Owner.Creature,  
                null,               
                false               
            );
        }
    }
}