using System.Collections.Generic;
using System.Threading.Tasks;
using BaseLib.Abstracts;
using BaseLib.Utils;
using KomeijiKoishi.Pools;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;

namespace KomeijiKoishi.Relics
{
    [Pool(typeof(KoishiSharedRelicPool))]
    public sealed class NightSparrowWings_Koishi : CustomRelicModel
    {
        public override RelicRarity Rarity => RelicRarity.Event;

        public override string PackedIconPath => "res://mods/Komeiji_Koishi/images/relics/NightSparrowWings_Koishi.png";

        protected override string PackedIconOutlinePath => "res://mods/Komeiji_Koishi/images/relics/NightSparrowWings_Koishi.png";

        protected override string BigIconPath => "res://mods/Komeiji_Koishi/images/relics/NightSparrowWings_Koishi.png";

        protected override IEnumerable<DynamicVar> CanonicalVars => new DynamicVar[]
        {
            new HealVar(1m)
        };

        public override async Task AfterDeath(PlayerChoiceContext choiceContext, Creature creature, bool wasRemovalPrevented, float deathAnimLength)
        {
            if (wasRemovalPrevented || creature.Side == base.Owner.Creature.Side || base.Owner.Creature.IsDead)
            {
                return;
            }

            base.Flash();
            await CreatureCmd.Heal(base.Owner.Creature, base.DynamicVars.Heal.BaseValue, true);
        }
    }
}
