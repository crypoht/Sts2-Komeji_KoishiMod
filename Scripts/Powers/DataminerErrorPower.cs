using BaseLib.Abstracts;
using BaseLib.Hooks;
using KomeijiKoishi.Patches;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.Localization;

namespace KomeijiKoishi.Powers;

// A combat-only hand-size modifier used exclusively by Dataminer outcomes.
public sealed class DataminerErrorPower : CustomPowerModel, IMaxHandSizeModifier
{
    public override LocString Description => new(
        "powers",
        DataminerDescriptionPatch.ShowReadableDescription
            ? "KOMEIJIKOISHI-DATAMINER_ERROR_POWER.readableDescription"
            : "KOMEIJIKOISHI-DATAMINER_ERROR_POWER.description");

    public override PowerType Type => PowerType.Buff;

    public override PowerStackType StackType => PowerStackType.Counter;

    public override string? CustomPackedIconPath => "res://mods/Komeiji_Koishi/images/powers/ErrorPower.png";

    public override string? CustomBigIconPath => "res://mods/Komeiji_Koishi/images/powers/ErrorPower.png";

    public int ModifyMaxHandSize(Player player, int currentMaxHandSize)
    {
        return player.Creature == base.Owner
            ? System.Math.Max(5, currentMaxHandSize + (int)base.Amount)
            : currentMaxHandSize;
    }
}
