using System.Collections.Generic;
using System.Threading.Tasks;
using BaseLib.Abstracts;
using BaseLib.Utils;
using KomeijiKoishi.Pools;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Potions;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.Factories;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Saves.Runs;

namespace KomeijiKoishi.Relics
{
    [Pool(typeof(KoishiSharedRelicPool))]
    public sealed class MagicPotionBottle_Koishi : CustomRelicModel
    {
        public override RelicRarity Rarity => RelicRarity.Event;

        public override bool ShowCounter => true;

        public override int DisplayAmount => this.KillsUntilPotion;

        public override string PackedIconPath => "res://mods/Komeiji_Koishi/images/relics/MagicPotionBottle_Koishi.png";

        protected override string PackedIconOutlinePath => "res://mods/Komeiji_Koishi/images/relics/MagicPotionBottle_Koishi.png";

        protected override string BigIconPath => "res://mods/Komeiji_Koishi/images/relics/MagicPotionBottle_Koishi.png";

        protected override IEnumerable<DynamicVar> CanonicalVars => new DynamicVar[]
        {
            new DynamicVar("Kills", 3m)
        };

        private int killsUntilPotion = 3;

        [SavedProperty]
        public int KillsUntilPotion
        {
            get => this.killsUntilPotion;
            set
            {
                base.AssertMutable();
                this.killsUntilPotion = value;
                base.InvokeDisplayAmountChanged();
            }
        }

        public override async Task AfterDeath(PlayerChoiceContext choiceContext, Creature creature, bool wasRemovalPrevented, float deathAnimLength)
        {
            if (wasRemovalPrevented || creature.Side == base.Owner.Creature.Side || base.Owner.Creature.IsDead)
            {
                return;
            }

            this.KillsUntilPotion--;
            if (this.KillsUntilPotion > 0)
            {
                return;
            }

            base.Flash();
            this.KillsUntilPotion = base.DynamicVars["Kills"].IntValue;

            PotionModel potion = PotionFactory.CreateRandomPotionOutOfCombat(
                base.Owner,
                base.Owner.RunState.Rng.CombatPotionGeneration,
                null
            ).ToMutable();
            await PotionCmd.TryToProcure(potion, base.Owner, -1);
        }
    }
}

