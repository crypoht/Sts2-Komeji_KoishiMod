using MegaCrit.Sts2.Core.Models;

namespace KomeijiKoishi.Modifiers
{
    public sealed class DisableKoishiAncientWeightsModifier : ModifierModel
    {
        public override bool ShouldReceiveCombatHooks => false;
    }
}
