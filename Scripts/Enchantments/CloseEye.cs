using BaseLib.Abstracts;
using KomeijiKoishi.Cards;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Models;

namespace KomeijiKoishi.Enchantments
{
    public sealed class CloseEye : CustomEnchantmentModel
    {
        private const decimal LockedCost = 514m;

        public override bool HasExtraCardText => false;

        protected override string? CustomIconPath => "res://mods/Komeiji_Koishi/images/enchantments/CloseEye.png";

        public override bool CanEnchant(CardModel card)
        {
            return card is CompleteUnconscious_Koishi && base.CanEnchant(card);
        }

        public override bool TryModifyEnergyCostInCombat(CardModel card, decimal originalCost, out decimal modifiedCost)
        {
            return TryLockCost(card, originalCost, out modifiedCost);
        }

        public override bool TryModifyEnergyCostInCombatLate(CardModel card, decimal originalCost, out decimal modifiedCost)
        {
            return TryLockCost(card, originalCost, out modifiedCost);
        }

        private bool TryLockCost(CardModel card, decimal originalCost, out decimal modifiedCost)
        {
            if (card == base.Card)
            {
                modifiedCost = LockedCost;
                return true;
            }

            modifiedCost = originalCost;
            return false;
        }
    }
}
