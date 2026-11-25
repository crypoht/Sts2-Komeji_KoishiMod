using BaseLib.Utils;
using KomeijiKoishi.Pools;
using KomeijiKoishi.Utils_Koishi;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Relics;
#if STS2_BETA
using MegaCrit.Sts2.Core.Runs;
#endif

namespace KomeijiKoishi.Relics;

[Pool(typeof(KoishiSharedRelicPool))]
public sealed class UnconsciousYoukaiMousse_Koishi : KoishiCollaborationRelic
{
    private const int PlayLimit = 13;
    private bool _checkingPlayability;
    private bool _drawing;
    private int _pendingDraws;

    protected override IEnumerable<DynamicVar> CanonicalVars => [new EnergyVar(2)];
    protected override IEnumerable<IHoverTip> ExtraHoverTips => [HoverTipFactory.ForEnergy(this)];

    public override decimal ModifyMaxEnergy(Player player, decimal amount) =>
        player == Owner ? amount + DynamicVars.Energy.BaseValue : amount;

    public override bool ShouldPlay(CardModel card, AutoPlayType autoPlayType) =>
        card.Owner != Owner || autoPlayType != AutoPlayType.None || _checkingPlayability;

    public override async Task AfterAutoPrePlayPhaseEnteredLate(PlayerChoiceContext choiceContext, Player player)
    {
        if (player != Owner || player.Creature.CombatState is not { } combat || player.PlayerCombatState is not { } state)
            return;

        int turn = state.TurnNumber;
#if STS2_BETA
        using var selector = RunManager.Instance.CardSelectorStack.PushSelector(player, new VakuuCardSelector());
#else
        using var selector = CardSelectCmd.PushSelector(new VakuuCardSelector());
#endif
        for (int plays = 0; plays < PlayLimit && state.TurnNumber == turn && CanContinue(player) &&
            !CombatManager.Instance.IsPlayerReadyToEndTurn(player); plays++)
        {
            CardModel? card = GetPlayableCard(player);
            if (card == null)
                break;

            Creature? target = card.TargetType switch
            {
                TargetType.AnyEnemy => combat.HittableEnemies.FirstOrDefault(),
                TargetType.AnyPlayer => player.Creature,
                TargetType.AnyAlly => player.RunState.Rng.CombatTargets.NextItem(
                    combat.PlayerCreatures.Where(creature => creature != player.Creature && creature.IsAlive)),
                _ => null
            };

            await card.SpendResources();
            bool added = KoishiExtensions.AutoPlayedByUnconsciousCards.Add(card);
            try
            {
                await CardCmd.AutoPlay(choiceContext, card, target, AutoPlayType.Default, true, false);
            }
            finally
            {
                if (added)
                    KoishiExtensions.AutoPlayedByUnconsciousCards.Remove(card);
            }
        }
    }

    public override async Task AfterCardPlayed(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        if (cardPlay.Card.Owner != Owner || !CanContinue(Owner))
            return;

        _pendingDraws++;
        if (_drawing)
            return;

        // Draw-triggered auto-plays can request another draw before the current draw finishes.
        _drawing = true;
        try
        {
            for (int draws = 0; _pendingDraws > 0 && draws < PlayLimit && CanContinue(Owner); draws++)
            {
                _pendingDraws--;
                await CardPileCmd.Draw(choiceContext, 1m, Owner);
            }
        }
        finally
        {
            _drawing = false;
            _pendingDraws = 0;
        }
    }

    private CardModel? GetPlayableCard(Player player)
    {
        _checkingPlayability = true;
        try
        {
            return PileType.Hand.GetPile(player).Cards.FirstOrDefault(card => card.CanPlay());
        }
        finally
        {
            _checkingPlayability = false;
        }
    }

    private static bool CanContinue(Player player) => player.Creature.IsAlive && !CombatManager.Instance.IsOverOrEnding;
}
