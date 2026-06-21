using System.Collections.Generic;
using System.Threading.Tasks;
using BaseLib.Abstracts;
using BaseLib.Utils;
using KomeijiKoishi.Pools;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.Relics;
using MegaCrit.Sts2.Core.ValueProps;

namespace KomeijiKoishi.Relics
{
    [Pool(typeof(KoishiSharedRelicPool))]
    public sealed class KanakosShimenawa_Koishi : CustomRelicModel
    {
        public override RelicRarity Rarity => RelicRarity.Ancient;

        protected override IEnumerable<DynamicVar> CanonicalVars => new List<DynamicVar>
        {
            new DamageVar(2m, ValueProp.Unpowered)
        };

        public override string PackedIconPath => $"res://mods/Komeiji_Koishi/images/relics/KanakosShimenawa_Koishi.png";
        protected override string PackedIconOutlinePath => $"res://mods/Komeiji_Koishi/images/relics/KanakosShimenawa_Koishi.png";
        protected override string BigIconPath => $"res://mods/Komeiji_Koishi/images/relics/KanakosShimenawa_Koishi.png";

        public override async Task AfterCardPlayed(PlayerChoiceContext choiceContext, CardPlay cardPlay)
        {
            if (cardPlay.Card.Owner != base.Owner)
            {
                return;
            }

            base.Flash();

            var combatState = base.Owner.Creature.CombatState;
            if (combatState == null)
            {
                return;
            }

            await CreatureCmd.Damage(
                choiceContext,
                combatState.HittableEnemies,
                base.DynamicVars.Damage,
                base.Owner.Creature
            );
        }
    }
}
