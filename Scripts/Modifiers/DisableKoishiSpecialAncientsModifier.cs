using MegaCrit.Sts2.Core.Models;

namespace KomeijiKoishi.Modifiers;

public sealed class DisableKoishiSpecialAncientsModifier : ModifierModel
{
    public override bool ShouldReceiveCombatHooks => false;
}
