using System.Collections.Generic;
using System.Threading.Tasks;
using BaseLib.Abstracts;
using KomeijiKoishi.Cards;
using KomeijiKoishi.Dataminer;
using KomeijiKoishi.Multiplayer;
using KomeijiKoishi.Patches;
using MegaCrit.Sts2.Core.CardSelection;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Cards;
using MegaCrit.Sts2.Core.Models.Orbs;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.Runs;
using MegaCrit.Sts2.Core.Saves.Runs;
using MegaCrit.Sts2.Core.ValueProps;

namespace KomeijiKoishi.Powers;

public sealed class DataminerAbilityPower : CustomPowerModel
{
    private bool _isResolvingDataminerAbility;

    public DataminerAbilityTriggerKind Trigger { get; private set; }

    public DataminerEffectKind EffectKind { get; private set; }

    public int EffectAmount { get; private set; }

    public string? EffectPowerId { get; private set; }

    public DataminerPileScope? EffectPileScope { get; private set; }

    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Single;
    public override PowerInstanceType InstanceType => PowerInstanceType.Instanced;

    public override LocString Description
    {
        get
        {
            LocString description = new(
                "powers",
                DataminerDescriptionPatch.ShowReadableDescription
                    ? "KOMEIJIKOISHI-DATAMINER_ABILITY_POWER.readableDescription"
                    : "KOMEIJIKOISHI-DATAMINER_ABILITY_POWER.description");

            if (DataminerDescriptionPatch.ShowReadableDescription)
            {
                description.Add("Trigger", DataminerDescriptionPatch.BuildAbilityTrigger(Trigger));
                description.Add("Effect", DataminerDescriptionPatch.BuildSubEffect(
                    new DataminerSubEffect(EffectKind, EffectAmount, EffectPowerId, pileScope: EffectPileScope)));
            }

            return description;
        }
    }

    public override string? CustomPackedIconPath =>
        "res://mods/Komeiji_Koishi/images/powers/ErrorPower.png";

    public override string? CustomBigIconPath =>
        "res://mods/Komeiji_Koishi/images/powers/ErrorPower.png";

    public void Configure(DataminerAbilityTriggerKind trigger, DataminerSubEffect effect)
    {
        base.AssertMutable();
        Trigger = trigger;
        EffectKind = effect.Kind;
        EffectAmount = effect.Amount;
        EffectPowerId = effect.PowerId;
        EffectPileScope = effect.PileScope;
    }

    public DataminerSubEffect CurrentEffect =>
        new(EffectKind, EffectAmount, EffectPowerId, pileScope: EffectPileScope);

    public override async Task BeforeApplied(
        Creature target,
        decimal amount,
        Creature? applier,
        CardModel? cardSource)
    {
        if (Trigger == DataminerAbilityTriggerKind.None && cardSource is not DataminerCard_Koishi)
        {
            await TriggerEffect(new ThrowingPlayerChoiceContext());
        }
    }

    public override async Task AfterDamageReceived(
        PlayerChoiceContext context,
        Creature target,
        DamageResult result,
        ValueProp props,
        Creature? dealer,
        CardModel? cardSource)
    {
        if (Trigger == DataminerAbilityTriggerKind.OnLoseHealth
            && target == base.Owner
            && result.UnblockedDamage > 0
            && cardSource is not DataminerCard_Koishi)
        {
            await TriggerEffect(context);
        }
    }

    public override async Task AfterCardExhausted(
        PlayerChoiceContext context,
        CardModel card,
        bool causedByEthereal)
    {
        if (Trigger == DataminerAbilityTriggerKind.OnExhaustCard
            && card.Owner?.Creature == base.Owner
            && card is not DataminerCard_Koishi)
        {
            await TriggerEffect(context);
        }
    }

    public override async Task AfterCardDrawn(
        PlayerChoiceContext context,
        CardModel card,
        bool fromHandDraw)
    {
        if (card.Owner?.Creature != base.Owner
            || card is DataminerCard_Koishi)
        {
            return;
        }

        bool matches = Trigger switch
        {
            DataminerAbilityTriggerKind.OnDrawCard => true,
            DataminerAbilityTriggerKind.OnDrawEthereal => card.Keywords.Contains(CardKeyword.Ethereal),
            DataminerAbilityTriggerKind.OnDrawStatus => card.Type == CardType.Status,
            _ => false
        };

        if (matches)
        {
            await TriggerEffect(context);
        }
    }

    public override async Task AfterCardDrawnEarly(
        PlayerChoiceContext context,
        CardModel card,
        bool fromHandDraw)
    {
        if (Trigger == DataminerAbilityTriggerKind.OnDrawStrike
            && card.Owner?.Creature == base.Owner
            && card is not DataminerCard_Koishi
            && (card.Id.Entry.Contains("STRIKE", StringComparison.OrdinalIgnoreCase)
                || card.Title.Contains("打击", StringComparison.Ordinal)))
        {
            await TriggerEffect(context);
        }
    }

    public override async Task AfterBlockGained(
        Creature creature,
        decimal amount,
        ValueProp props,
        CardModel? cardSource)
    {
        if (Trigger == DataminerAbilityTriggerKind.OnGainBlock
            && creature == base.Owner
            && amount > 0
            && cardSource is not DataminerCard_Koishi)
        {
            await TriggerEffect(new ThrowingPlayerChoiceContext());
        }
    }

    public override async Task AfterCardGeneratedForCombat(
        CardModel card,
        Player? creator)
    {
        if (Trigger == DataminerAbilityTriggerKind.OnGenerateCard
            && creator?.Creature == base.Owner
            && card is not DataminerCard_Koishi)
        {
            await TriggerEffect(new ThrowingPlayerChoiceContext());
        }
        else if (Trigger == DataminerAbilityTriggerKind.OnGenerateStatus
            && creator?.Creature == base.Owner
            && card is not DataminerCard_Koishi
            && card.Type == CardType.Status)
        {
            await TriggerEffect(new ThrowingPlayerChoiceContext());
        }
    }

    public override async Task AfterEnergySpent(CardModel card, int amount)
    {
        if (Trigger == DataminerAbilityTriggerKind.OnSpendEnergy
            && card.Owner?.Creature == base.Owner
            && amount > 0
            && card is not DataminerCard_Koishi)
        {
            await TriggerEffect(new ThrowingPlayerChoiceContext());
        }
    }

    public override async Task AfterOrbChanneled(
        PlayerChoiceContext context,
        Player player,
        OrbModel orb)
    {
        if (Trigger == DataminerAbilityTriggerKind.OnChannelLightning
            && player.Creature == base.Owner
            && orb is LightningOrb)
        {
            await TriggerEffect(context);
        }
    }

    public override async Task AfterShuffle(
        PlayerChoiceContext context,
        Player shuffler)
    {
        if (Trigger == DataminerAbilityTriggerKind.OnShuffleDrawPile
            && shuffler.Creature == base.Owner)
        {
            await TriggerEffect(context);
        }
    }

    public override async Task AfterStarsGained(int amount, Player gainer)
    {
        if (Trigger == DataminerAbilityTriggerKind.OnSpendOrGainStars
            && gainer.Creature == base.Owner
            && amount > 0)
        {
            await TriggerEffect(new ThrowingPlayerChoiceContext());
        }
    }

    public override async Task AfterStarsSpent(int amount, Player spender)
    {
        if (Trigger == DataminerAbilityTriggerKind.OnSpendOrGainStars
            && spender.Creature == base.Owner
            && amount > 0)
        {
            await TriggerEffect(new ThrowingPlayerChoiceContext());
        }
    }

    public override async Task AfterPowerAmountChanged(
        PlayerChoiceContext context,
        PowerModel power,
        decimal amount,
        Creature? applier,
        CardModel? cardSource)
    {
        if (amount <= 0
            || cardSource is DataminerCard_Koishi
            || applier != base.Owner)
        {
            return;
        }

        if (Trigger == DataminerAbilityTriggerKind.OnGiveVulnerable
            && power is VulnerablePower)
        {
            await TriggerEffect(context);
        }
        else if (Trigger == DataminerAbilityTriggerKind.OnGiveEnemyDebuff
            && power.TypeForCurrentAmount == PowerType.Debuff
            && power.Owner?.Side == CombatSide.Enemy)
        {
            await TriggerEffect(context, power.Owner);
        }
    }

    public override async Task AfterCardPlayed(
        PlayerChoiceContext context,
        CardPlay cardPlay)
    {
        MegaCrit.Sts2.Core.Logging.Log.Info(
            $"[KoishiDataminerAbility] AfterCardPlayed trigger={Trigger}, card={cardPlay.Card.Id.Entry}, ownerMatch={cardPlay.Card.Owner?.Creature == base.Owner}");

        if (cardPlay.Card is DataminerCard_Koishi
            || cardPlay.Card.Owner?.Creature != base.Owner)
        {
            return;
        }

        bool matches = Trigger switch
        {
            DataminerAbilityTriggerKind.OnPlayCard => true,
            DataminerAbilityTriggerKind.OnPlaySkill => cardPlay.Card.Type == CardType.Skill,
            DataminerAbilityTriggerKind.OnPlayAttack => cardPlay.Card.Type == CardType.Attack,
            DataminerAbilityTriggerKind.OnPlayPower => cardPlay.Card.Type == CardType.Power,
            DataminerAbilityTriggerKind.OnPlayStatus => cardPlay.Card.Type == CardType.Status,
            DataminerAbilityTriggerKind.OnPlayCurse => cardPlay.Card.Type == CardType.Curse,
            DataminerAbilityTriggerKind.OnPlaySoul => cardPlay.Card is Soul,
            DataminerAbilityTriggerKind.OnPlayEthereal => cardPlay.Card.Keywords.Contains(CardKeyword.Ethereal),
            _ => false
        };

        if (matches)
        {
            MegaCrit.Sts2.Core.Logging.Log.Info(
                $"[KoishiDataminerAbility] AfterCardPlayed matched trigger={Trigger}; queueing effect.");
            QueueTriggeredEffect(cardPlay.Target);
        }
    }

    public override async Task AfterPlayerTurnStart(
        PlayerChoiceContext context,
        Player player)
    {
        if (player.Creature == base.Owner
            && Trigger == DataminerAbilityTriggerKind.OnTurnStart)
        {
            await TriggerEffect(context);
        }
    }

    public override async Task AfterSideTurnEnd(
        PlayerChoiceContext context,
        CombatSide side,
        IEnumerable<Creature> participants)
    {
        if (side == base.Owner.Side
            && participants.Contains(base.Owner)
            && Trigger == DataminerAbilityTriggerKind.OnTurnEnd)
        {
            await TriggerEffect(context);
        }
    }

    public override async Task AfterDamageGiven(
        PlayerChoiceContext context,
        Creature? dealer,
        DamageResult result,
        ValueProp props,
        Creature target,
        CardModel? cardSource)
    {
        if (dealer == base.Owner
            && cardSource is not DataminerCard_Koishi
            && result.UnblockedDamage > 0
            && Trigger is DataminerAbilityTriggerKind.OnUnblockedDamage
                or DataminerAbilityTriggerKind.OnAttack)
        {
            await TriggerEffect(context, target);
        }
    }

    public override async Task AfterCombatEnd(MegaCrit.Sts2.Core.Rooms.CombatRoom room)
    {
        if (Trigger == DataminerAbilityTriggerKind.OnCombatEnd)
        {
            await TriggerEffect(new ThrowingPlayerChoiceContext());
        }
    }

    private async Task TriggerEffect(PlayerChoiceContext context, Creature? preferredTarget = null)
    {
        if (_isResolvingDataminerAbility
            || base.Owner?.Player is not { } player
            || base.CombatState == null)
        {
            return;
        }

        _isResolvingDataminerAbility = true;
        try
        {
            // Ability effects never require player selection. Use a fresh context so
            // commands triggered from a hook do not wait on the hook's own action.
            PlayerChoiceContext effectContext = new ThrowingPlayerChoiceContext();
            base.Flash();
            Creature? enemy = preferredTarget
                ?? player.Creature.CombatState?.HittableEnemies.FirstOrDefault();

            switch (EffectKind)
            {
                case DataminerEffectKind.Damage when enemy != null:
#if STS2_BETA
                    await CreatureCmd.Damage(
                        effectContext,
                        enemy,
                        EffectAmount,
                        ValueProp.Move,
                        null,
                        null);
#else
                    await CreatureCmd.Damage(
                        effectContext,
                        enemy,
                        EffectAmount,
                        ValueProp.Move,
                        player.Creature,
                        null);
#endif
                    break;
                case DataminerEffectKind.Block:
                    await CreatureCmd.GainBlock(player.Creature, EffectAmount, ValueProp.Move, null, false);
                    break;
                case DataminerEffectKind.Draw:
                    await CardPileCmd.Draw(effectContext, EffectAmount, player, false);
                    break;
                case DataminerEffectKind.Energy:
                    if (EffectAmount >= 0)
                    {
                        await PlayerCmd.GainEnergy(EffectAmount, player);
                    }
                    else
                    {
                        await PlayerCmd.LoseEnergy(-EffectAmount, player);
                    }
                    break;
                case DataminerEffectKind.Health:
                    if (EffectAmount >= 0)
                    {
                        await CreatureCmd.Heal(player.Creature, EffectAmount, true);
                    }
                    else
                    {
                        await CreatureCmd.Damage(
                            effectContext,
                            player.Creature,
                            -EffectAmount,
                            ValueProp.Unblockable | ValueProp.Unpowered,
                            null,
                            null);
                    }
                    break;
                case DataminerEffectKind.Poison when enemy != null:
                    await PowerCmd.Apply<PoisonPower>(
                        effectContext,
                        enemy,
                        EffectAmount,
                        player.Creature,
                        null,
                        false);
                    break;
                case DataminerEffectKind.RandomPower:
                    if (EffectPowerId is { } powerId
                        && DataminerPowerPool.Resolve(powerId) is { } power)
                    {
                    await PowerCmd.Apply(
                            effectContext,
                            power.ToMutable(),
                            player.Creature,
                            EffectAmount,
                            player.Creature,
                            null,
                            false);
                    }
                    break;
                case DataminerEffectKind.GenerateShivs:
                    await Shiv.CreateInHand(player, EffectAmount, base.CombatState);
                    break;
                case DataminerEffectKind.GenerateSouls:
                    await Soul.CreateInHand(player, EffectAmount, base.CombatState);
                    break;
                case DataminerEffectKind.ChannelOrb:
                    await ChannelOrb(effectContext, player);
                    break;
                case DataminerEffectKind.InstantDeath:
                    await CreatureCmd.Kill(player.Creature, false);
                    break;
                case DataminerEffectKind.DirectWin:
                    foreach (Creature target in player.Creature.CombatState!.Enemies.ToList())
                    {
                        await CreatureCmd.Kill(target, false);
                    }
                    await CombatManager.Instance.CheckWinCondition();
                    break;
            }
        }
        catch (Exception exception)
        {
            MegaCrit.Sts2.Core.Logging.Log.Error(
                $"[KoishiDataminerAbility] Effect failed safely. trigger={Trigger}, effect={EffectKind}: {exception}");
        }
        finally
        {
            _isResolvingDataminerAbility = false;
        }
    }

    private void QueueTriggeredEffect(Creature? preferredTarget)
    {
        if (_isResolvingDataminerAbility
            || base.Owner?.Player is not { } player
            || base.CombatState == null)
        {
            MegaCrit.Sts2.Core.Logging.Log.Info(
                "[KoishiDataminerAbility] QueueSkipped: resolving, missing player, or missing combat state.");
            return;
        }

        if (!MegaCrit.Sts2.Core.Context.LocalContext.IsMe(player))
        {
            MegaCrit.Sts2.Core.Logging.Log.Info(
                $"[KoishiDataminerAbility] QueueSkipped: non-local player {player.NetId}.");
            return;
        }

        int powerOrdinal = player.Creature.Powers
            .OfType<DataminerAbilityPower>()
            .ToList()
            .IndexOf(this);
        if (powerOrdinal < 0)
        {
            MegaCrit.Sts2.Core.Logging.Log.Warn(
                "[KoishiDataminerAbility] QueueSkipped: ability was not found in the owner's ability list.");
            return;
        }

        MegaCrit.Sts2.Core.Logging.Log.Info(
            $"[KoishiDataminerAbility] RequestEnqueue powerOrdinal={powerOrdinal}, trigger={Trigger}, effect={EffectKind}.");
        RunManager.Instance.ActionQueueSynchronizer.RequestEnqueue(
            new DataminerAbilityTriggerGameAction(player, powerOrdinal, Trigger, CurrentEffect));
    }

    public Task ResolveQueuedEffect(DataminerAbilityTriggerKind trigger, DataminerSubEffect effect)
    {
        Configure(trigger, effect);
        MegaCrit.Sts2.Core.Logging.Log.Info(
            $"[KoishiDataminerAbility] ResolveQueuedEffect trigger={Trigger}, effect={EffectKind}, amount={EffectAmount}.");
        return TriggerEffect(new ThrowingPlayerChoiceContext());
    }

    private async Task ChannelOrb(PlayerChoiceContext context, Player player)
    {
        switch (EffectPowerId)
        {
            case nameof(DataminerOrbKind.Lightning):
                await OrbCmd.Channel<LightningOrb>(context, player);
                break;
            case nameof(DataminerOrbKind.Glass):
                await OrbCmd.Channel<GlassOrb>(context, player);
                break;
            case nameof(DataminerOrbKind.Dark):
                await OrbCmd.Channel<DarkOrb>(context, player);
                break;
            default:
                await OrbCmd.Channel<FrostOrb>(context, player);
                break;
        }
    }
}
