using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Saves.Runs;

namespace KomeijiKoishi.Modifiers;

public sealed class KoishiSpineModeModifier : ModifierModel
{
    public override bool ShouldReceiveCombatHooks => false;

    [SavedProperty]
    public bool PremiumAnimation { get; private set; }

    public void SetMode(KoishiSpineMode mode)
    {
        PremiumAnimation = mode == KoishiSpineMode.PremiumAnimation;
    }
}
