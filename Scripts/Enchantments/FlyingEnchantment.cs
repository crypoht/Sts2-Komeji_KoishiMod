using System.Threading.Tasks;
using BaseLib.Abstracts;
using KomeijiKoishi.Powers;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;

namespace KomeijiKoishi.Enchantments
{
    public sealed class FlyingEnchantment : CustomEnchantmentModel
    {
        public override bool HasExtraCardText => true;

        protected override string? CustomIconPath => "res://mods/Komeiji_Koishi/images/enchantments/FlyingEnchantment.png";

        public override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay? cardPlay)
        {
            await PowerCmd.Apply<FlyingDamageReductionPower>(
                choiceContext,
                base.Card.Owner.Creature,
                1m,
                base.Card.Owner.Creature,
                base.Card,
                false
            );
        }
    }
}
