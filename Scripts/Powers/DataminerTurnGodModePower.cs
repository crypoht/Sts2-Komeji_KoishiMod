using System.Threading.Tasks;
using BaseLib.Abstracts;
using KomeijiKoishi.Patches;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.ValueProps;

namespace KomeijiKoishi.Powers;

// The turn-only variant cannot reuse the console's permanent stat bundles:
// it instead prevents all incoming damage and removes itself at turn end.
public sealed class DataminerTurnGodModePower : CustomPowerModel
{
    public override LocString Description => new(
        "powers",
        DataminerDescriptionPatch.ShowReadableDescription
            ? "KOMEIJIKOISHI-DATAMINER_TURN_GOD_MODE_POWER.readableDescription"
            : "KOMEIJIKOISHI-DATAMINER_TURN_GOD_MODE_POWER.description");

    public override PowerType Type => PowerType.Buff;

    public override PowerStackType StackType => PowerStackType.Single;

    public override string? CustomPackedIconPath => "res://mods/Komeiji_Koishi/images/powers/ErrorPower.png";

    public override string? CustomBigIconPath => "res://mods/Komeiji_Koishi/images/powers/ErrorPower.png";

#if STS2_BETA
    public override decimal ModifyDamageMultiplicative(
        Creature? target,
        decimal amount,
        ValueProp props,
        Creature? dealer,
        CardModel? cardSource,
        CardPlay? cardPlay)
#else
    public override decimal ModifyDamageMultiplicative(
        Creature? target,
        decimal amount,
        ValueProp props,
        Creature? dealer,
        CardModel? cardSource)
#endif
    {
        return target == base.Owner ? 0m : 1m;
    }

    public override async Task AfterPlayerTurnStart(
        PlayerChoiceContext choiceContext,
        Player player)
    {
        if (player == base.Owner.Player)
        {
            await PowerCmd.Remove(this);
        }
    }
}
