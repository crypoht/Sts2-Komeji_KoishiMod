using System.Collections.Generic;
using System.Threading.Tasks;
using BaseLib.Abstracts;
using BaseLib.Utils;
using KomeijiKoishi.Pools;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Relics;
using MegaCrit.Sts2.Core.ValueProps;

namespace KomeijiKoishi.Relics
{
    [Pool(typeof(KoishiSharedRelicPool))]
    public sealed class MiniSaisenBox_Koishi : CustomRelicModel
    {
        private const string BonusPercentKey = "BonusPercent";

        public override RelicRarity Rarity => RelicRarity.Ancient;

        protected override IEnumerable<DynamicVar> CanonicalVars => new List<DynamicVar>
        {
            new DynamicVar(BonusPercentKey, 40m)
        };

        public override string PackedIconPath => "res://mods/Komeiji_Koishi/images/relics/MiniSaisenBox_Koishi.png";
        protected override string PackedIconOutlinePath => "res://mods/Komeiji_Koishi/images/relics/MiniSaisenBox_Koishi.png";
        protected override string BigIconPath => "res://mods/Komeiji_Koishi/images/relics/MiniSaisenBox_Koishi.png";

        private decimal BonusMultiplier => 1m + base.DynamicVars[BonusPercentKey].BaseValue / 100m;

        public override decimal ModifyGoldGained(Player player, decimal amount)
        {
            if (player != base.Owner)
            {
                return amount;
            }

            return 0m;
        }

        public override Task AfterModifyingGoldGained(Player player, decimal amount)
        {
            if (player == base.Owner && amount > 0m)
            {
                base.Flash();
            }

            return Task.CompletedTask;
        }

#if STS2_BETA
        public override decimal ModifyDamageMultiplicative(Creature? target, decimal amount, ValueProp props, Creature? dealer, CardModel? cardSource, CardPlay? cardPlay)
        {
            return ModifyDamageMultiplicativeCore(target, props, dealer, cardSource);
        }
#else
        public override decimal ModifyDamageMultiplicative(Creature? target, decimal amount, ValueProp props, Creature? dealer, CardModel? cardSource)
        {
            return ModifyDamageMultiplicativeCore(target, props, dealer, cardSource);
        }
#endif

        private decimal ModifyDamageMultiplicativeCore(Creature? target, ValueProp props, Creature? dealer, CardModel? cardSource)
        {
            if (dealer != base.Owner.Creature || target == null || target.Side == dealer.Side)
            {
                return 1m;
            }

            if (cardSource == null || cardSource.Type == CardType.Curse || cardSource.Type == CardType.Status)
            {
                return 1m;
            }

            if (!props.IsPoweredAttack())
            {
                return 1m;
            }

            return this.BonusMultiplier;
        }

        public override decimal ModifyBlockMultiplicative(Creature target, decimal block, ValueProp props, CardModel? cardSource, CardPlay? cardPlay)
        {
            if (target != base.Owner.Creature)
            {
                return 1m;
            }

            return this.BonusMultiplier;
        }
    }
}
