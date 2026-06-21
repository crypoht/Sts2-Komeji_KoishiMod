using System.Collections.Generic;
using System.Threading.Tasks;
using BaseLib.Abstracts;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;

namespace KomeijiKoishi.Enchantments
{
    public sealed class MiracleEnchantment : CustomEnchantmentModel
    {
        public override bool ShowAmount => true;

        public override bool HasExtraCardText => true;

        protected override IEnumerable<DynamicVar> CanonicalVars => new List<DynamicVar>
        {
            new EnergyVar(2)
        };

        protected override IEnumerable<IHoverTip> ExtraHoverTips => new[]
        {
            HoverTipFactory.FromKeyword(CardKeyword.Exhaust)
        };

        protected override string? CustomIconPath => "res://mods/Komeiji_Koishi/images/enchantments/MiracleEnchantment.png";

        protected override void OnEnchant()
        {
            if (base.Card.Keywords == null || !base.Card.Keywords.Contains(CardKeyword.Exhaust))
            {
                base.Card.AddKeyword(CardKeyword.Exhaust);
            }
        }

        public override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay? cardPlay)
        {
            await PlayerCmd.GainEnergy(base.DynamicVars.Energy.BaseValue, base.Card.Owner);
        }
    }
}
