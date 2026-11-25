using BaseLib.Utils;
using KomeijiKoishi.Pools;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.Saves.Runs;

namespace KomeijiKoishi.Relics;

[Pool(typeof(KoishiSharedRelicPool))]
public sealed class KoishisTelephone_Koishi : KoishiCollaborationRelic
{
    private const int TurnsPerActivation = 5;
    private int _turnsSeen;

    public override bool ShowCounter => true;
    public override int DisplayAmount => TurnsSeen;

    [SavedProperty]
    public int TurnsSeen
    {
        get => _turnsSeen;
        private set
        {
            AssertMutable();
            _turnsSeen = value;
            InvokeDisplayAmountChanged();
        }
    }

    protected override IEnumerable<IHoverTip> ExtraHoverTips => [HoverTipFactory.FromPower<IntangiblePower>()];

    public override async Task AfterPlayerTurnStart(PlayerChoiceContext choiceContext, Player player)
    {
        if (player != Owner || player.Creature.IsDead || CombatManager.Instance.IsOverOrEnding)
            return;

        TurnsSeen = (TurnsSeen + 1) % TurnsPerActivation;
        if (TurnsSeen != 0)
            return;

        Flash();
        await PowerCmd.Apply<IntangiblePower>(choiceContext, player.Creature, 1m, player.Creature, null, false);
    }
}
