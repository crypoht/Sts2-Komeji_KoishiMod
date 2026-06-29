using MegaCrit.Sts2.Core.Models;

namespace KomeijiKoishi.Modifiers
{
    public sealed class DisableKoishiAncientsModifier : ModifierModel
    {
        public override bool ShouldReceiveCombatHooks => false;
    }
}
