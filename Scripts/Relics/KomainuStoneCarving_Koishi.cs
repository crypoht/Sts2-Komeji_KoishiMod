using System.Collections.Generic;
using System.Threading.Tasks;
using BaseLib.Abstracts;
using BaseLib.Utils;
using KomeijiKoishi.Pools;
using KomeijiKoishi.Powers;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.Relics;

namespace KomeijiKoishi.Relics
{
    [Pool(typeof(KoishiSharedRelicPool))]
    public sealed class KomainuStoneCarving_Koishi : CustomRelicModel
    {
        public override RelicRarity Rarity => RelicRarity.Ancient;

        protected override IEnumerable<DynamicVar> CanonicalVars => new List<DynamicVar>
        {
            new PowerVar<GuardianPower>(2m)
        };

        protected override IEnumerable<IHoverTip> ExtraHoverTips => new[]
        {
            HoverTipFactory.FromPower<GuardianPower>()
        };

        public override string PackedIconPath => "res://mods/Komeiji_Koishi/images/relics/KomainuStoneCarving_Koishi.png";
        protected override string PackedIconOutlinePath => "res://mods/Komeiji_Koishi/images/relics/KomainuStoneCarving_Koishi.png";
        protected override string BigIconPath => "res://mods/Komeiji_Koishi/images/relics/KomainuStoneCarving_Koishi.png";

        public override async Task BeforeCombatStart()
        {
            base.Flash();
            await PowerCmd.Apply<GuardianPower>(
                new ThrowingPlayerChoiceContext(),
                base.Owner.Creature,
                base.DynamicVars["GuardianPower"].BaseValue,
                base.Owner.Creature,
                null,
                false
            );
        }
    }
}
