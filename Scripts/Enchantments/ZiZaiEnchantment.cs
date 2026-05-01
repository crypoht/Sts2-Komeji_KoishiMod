using System.Threading.Tasks;
using BaseLib.Abstracts;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using KomeijiKoishi.Enums; 
using System.Threading.Tasks;
using BaseLib.Utils;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.Entities.Creatures; 
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.Localization;


namespace KomeijiKoishi.Enchantments
{
    public sealed class ZiZaiEnchantment : CustomEnchantmentModel
    {
        public override bool ShowAmount => false;

        public override bool HasExtraCardText => true;

        protected override string? CustomIconPath => "res://mods/Komeiji_Koishi/images/enchantments/ZiZai.png";

        public override bool CanEnchant(CardModel card)
        {
            if (base.CanEnchant(card))
            {
                if (card.Keywords != null && card.Keywords.Contains(KoishiKeywords.Unconscious))
                {
                    return false;
                }
                return true;
            }
            return false;
        }


        public override Task AfterPlayerTurnStart(PlayerChoiceContext choiceContext, Player player)
        {

            if (base.Card != null && player == base.Card.Owner)
            {
                if (base.Card.CombatState != null && base.Card.CombatState.RoundNumber == 1)
                {
                    CardCmd.ApplyKeyword(base.Card, new CardKeyword[] { KoishiKeywords.Unconscious });
                }
            }
            
            return Task.CompletedTask;
        }
    }
}