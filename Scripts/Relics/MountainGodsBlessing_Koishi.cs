using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BaseLib.Abstracts;
using BaseLib.Utils;
using KomeijiKoishi.Pools;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.Relics;
using MegaCrit.Sts2.Core.ValueProps;

namespace KomeijiKoishi.Relics
{
    [Pool(typeof(KoishiSharedRelicPool))]
    public sealed class MountainGodsBlessing_Koishi : CustomRelicModel
    {
        private const string HandThresholdKey = "HandThreshold";

        public override RelicRarity Rarity => RelicRarity.Ancient;

        protected override IEnumerable<DynamicVar> CanonicalVars => new List<DynamicVar>
        {
            new EnergyVar(1),
            new DamageVar(6m, ValueProp.Unblockable | ValueProp.Unpowered),
            new DynamicVar(HandThresholdKey, 1m)
        };

        protected override IEnumerable<IHoverTip> ExtraHoverTips => new[]
        {
            HoverTipFactory.ForEnergy(this)
        };

        public override string PackedIconPath => $"res://mods/Komeiji_Koishi/images/relics/MountainGodsBlessing_Koishi.png";
        protected override string PackedIconOutlinePath => $"res://mods/Komeiji_Koishi/images/relics/MountainGodsBlessing_Koishi.png";
        protected override string BigIconPath => $"res://mods/Komeiji_Koishi/images/relics/MountainGodsBlessing_Koishi.png";

        public override decimal ModifyMaxEnergy(Player player, decimal amount)
        {
            if (player != base.Owner)
            {
                return amount;
            }

            return amount + base.DynamicVars.Energy.BaseValue;
        }

        public override async Task BeforeSideTurnEnd(PlayerChoiceContext choiceContext, CombatSide side, IEnumerable<Creature> participants)
        {
            if (!participants.Contains(base.Owner.Creature))
            {
                return;
            }

            CardPile hand = PileType.Hand.GetPile(base.Owner);
            if (hand.Cards.Count > base.DynamicVars[HandThresholdKey].IntValue)
            {
                return;
            }

            base.Flash();
            await CreatureCmd.Damage(choiceContext, base.Owner.Creature, base.DynamicVars.Damage, null, null);
        }
    }
}
