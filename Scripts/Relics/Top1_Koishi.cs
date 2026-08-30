using BaseLib.Abstracts;
using BaseLib.Utils;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.RelicPools;
using MegaCrit.Sts2.Core.Models.Relics;

namespace KomeijiKoishi.Relics
{
    [Pool(typeof(EventRelicPool))]
    public sealed class Top1_Koishi : CustomRelicModel
    {
        public override RelicRarity Rarity => RelicRarity.Starter;

        // Reuse the existing Koishi relic icon until a dedicated Top1 icon is provided.
        public override string PackedIconPath => "res://mods/Komeiji_Koishi/images/relics/Top1_Koishi.png";
        protected override string PackedIconOutlinePath => "res://mods/Komeiji_Koishi/images/relics/Top1_Koishi.png";
        protected override string BigIconPath => "res://mods/Komeiji_Koishi/images/relics/Top1_Koishi.png";
    }
}
