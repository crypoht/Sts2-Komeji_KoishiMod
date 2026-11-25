using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Commands.Builders;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Hooks;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.ValueProps;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Localization.DynamicVars;

namespace KomeijiKoishi.Powers
{
    public sealed class BloomStancePower : KoishiStancePower
    {
        public override string? CustomPackedIconPath =>
            "res://mods/Komeiji_Koishi/images/powers/BloomStancePower.png";
        public override string? CustomBigIconPath =>
            "res://mods/Komeiji_Koishi/images/powers/BloomStancePower.png";

        public override LocString Description =>
            new("powers", base.Id.Entry + (KomeijiKoishi.Config.KoishiBalanceManager.IsEnabled ? ".balanceDescription" : ".description"));

        public static int BloomEnergyGainAmount = 1;

        private readonly Dictionary<CardModel, AttackCommand> _activeCardAttacks = new();

        private bool _isExecutingBloom = false;

        protected override object InitInternalData() => new object();

        protected override IEnumerable<IHoverTip> ExtraHoverTips =>
            new[] { HoverTipFactory.ForEnergy(this) };

        protected override IEnumerable<DynamicVar> CanonicalVars =>
            new List<DynamicVar> { new DynamicVar("BlockReduction", KomeijiKoishi.Config.KoishiBalanceManager.Value(60m, 70m)) };

        public decimal BlockReduction => KomeijiKoishi.Config.KoishiBalanceManager.Value(60m, 70m);

       
        public override decimal ModifyBlockMultiplicative(
            Creature target, decimal block, ValueProp props,
            CardModel? cardSource, CardPlay? cardPlay)
        {
            if (target == base.Owner)
            {
                if (base.Owner.GetPower<PhilosophyOfTheHatedPower>() != null)
                {
                    return 1m;
                }

                return 1m - (BlockReduction / 100m);
            }

            return 1m;
        }

        
        public static async Task EnterThisStance(
            PlayerChoiceContext context, Player player, CardModel sourceCard)
        {
            try
            {
                if (player.Creature.GetPower<BloomStancePower>() != null)
                {
                    await ClearOldStances(player, typeof(BloomStancePower));
                    return;
                }

                await ClearOldStances(player);

                int bonusEnergy = 0;
                var superego = player.Creature.Powers.FirstOrDefault(p => p is SuperegoPower);
                if (superego != null) bonusEnergy = (int)superego.Amount;

                int totalEnergyGain = BloomEnergyGainAmount + bonusEnergy;
                if (totalEnergyGain > 0)
                    await PlayerCmd.GainEnergy(totalEnergyGain, player);

                await PowerCmd.Apply<BloomStancePower>(
                    context,
                    player.Creature, 1m, player.Creature, sourceCard, false);

                await ClearOldStances(player, typeof(BloomStancePower));


                await NotifyAllCardsStanceChanged(player, "Bloom");
                await NotifyAllPowersStanceChanged(context, player, "Bloom", sourceCard);
            }
            catch (Exception e)
            {
                MegaCrit.Sts2.Core.Logging.Log.Error($"[BloomStance] Enter Error: {e}");
            }
        }

        
        public override Task BeforeAttack(AttackCommand command)
        {
            if (_isExecutingBloom)
            {
                return Task.CompletedTask;
            }

            if (command.ModelSource is not CardModel cardModel)
                return Task.CompletedTask;

            if (cardModel.Owner.Creature != base.Owner)
                return Task.CompletedTask;

            if (cardModel.Type != CardType.Attack)
                return Task.CompletedTask;

            if (!command.DamageProps.IsPoweredAttack())
                return Task.CompletedTask;

            _activeCardAttacks[cardModel] = command;
            return Task.CompletedTask;
        }

        public override Task AfterAttack(PlayerChoiceContext choiceContext, AttackCommand command)
        {
            if (command.ModelSource is CardModel cardModel
                && _activeCardAttacks.TryGetValue(cardModel, out var activeCommand)
                && activeCommand == command)
            {
                _activeCardAttacks.Remove(cardModel);
            }

            return Task.CompletedTask;
        }

        public override async Task AfterDamageGiven(
            PlayerChoiceContext choiceContext,
            Creature? dealer,
            DamageResult result,
            ValueProp props,
            Creature target,
            CardModel? cardSource)
        {
            if (_isExecutingBloom
                || dealer != base.Owner
                || cardSource is not CardModel cardModel
                || cardModel.Owner.Creature != base.Owner
                || cardModel.Type != CardType.Attack
                || !props.IsPoweredAttack()
                || result.TotalDamage <= 0)
            {
                return;
            }

            _activeCardAttacks.TryGetValue(cardModel, out var originalCommand);
            CardPlay? cardPlay = null;
#if STS2_BETA
            cardPlay = originalCommand?.CardPlay;
#endif
            var bloomContext = new ThrowingPlayerChoiceContext();
            await RunBloomAttackAsync(bloomContext, cardModel, cardPlay, target =>
            {
                decimal baseDamage = GetBloomDamageValue(cardModel, target);
                decimal damage = GetBloomModifiedDamage(cardModel, cardPlay, target, baseDamage);
                return CreatureCmd.Damage(bloomContext, target, damage, ValueProp.Unpowered, base.Owner);
            });
        }

        /// <summary>Replays one resolved attack-card hit from an allied external attacker.</summary>
        /// <remarks>
        /// Await once per original hit on the card owner's active Bloom. Suppress forwarding while
        /// resolving the replay. Pass the original context, amount before damage hooks, and flags.
        /// Stable damage commands use ModifyDamageHookType.All.
        /// </remarks>
        /// <returns>True if the replay recorded a hit. False does not authorize a fallback replay.</returns>
        public Task<bool> TryTriggerExternalAttackAsync(
            PlayerChoiceContext choiceContext,
            CardPlay cardPlay,
            Creature attacker,
            DamageResult result,
            decimal baseDamage,
            ValueProp props,
            ModifyDamageHookType modifyDamageHookType)
        {
            if (_isExecutingBloom
                || cardPlay.Card is not { Type: CardType.Attack, Owner: { } player } card
                || player.Creature != base.Owner
                || !base.Owner.IsAlive
                || base.Owner.GetPower<BloomStancePower>() != this
                || attacker == base.Owner
                || attacker.Side != base.Owner.Side
                || !attacker.IsAlive
                || base.CombatState is not { } combat
                || card.CombatState != combat
                || attacker.CombatState != combat
                || !props.IsPoweredAttack()
                || !result.Props.IsPoweredAttack()
                || result.TotalDamage <= 0
                || result.Receiver.Side == base.Owner.Side
#if STS2_BETA
                || result.OriginalTarget.Side == base.Owner.Side
#else
                || modifyDamageHookType != ModifyDamageHookType.All
#endif
                )
            {
                return Task.FromResult(false);
            }

            // Let the command modify raw damage once using the actual dealer and the new target.
            return RunBloomAttackAsync(choiceContext, card, cardPlay, target =>
#if STS2_BETA
                CreatureCmd.Damage(choiceContext, new[] { target }, baseDamage, props, attacker, card, cardPlay, modifyDamageHookType));
#else
                CreatureCmd.Damage(choiceContext, target, baseDamage, props, attacker, card));
#endif
        }

        private async Task<bool> RunBloomAttackAsync(
            PlayerChoiceContext choiceContext,
            CardModel cardModel,
            CardPlay? cardPlay,
            Func<Creature, Task<IEnumerable<DamageResult>>> dealDamage)
        {
            if (_isExecutingBloom
                || CombatManager.Instance is not { IsInProgress: true }
                || base.CombatState is not { } combat
                || base.Owner.Player is not { } player)
            {
                return false;
            }
#if STS2_BETA
            if (cardPlay == null)
                return false;
#endif

            _isExecutingBloom = true;
            try
            {
#if STS2_BETA
                await using var attackContext = await AttackCommand.CreateContextAsync(combat, choiceContext, cardPlay);
#else
                await using var attackContext = await AttackCommand.CreateContextAsync(combat, choiceContext, cardModel);
#endif
                var enemies = combat.GetOpponentsOf(base.Owner).Where(enemy => !enemy.IsDead).ToList();
                if (enemies.Count == 0)
                    return false;

                Creature? target = player.RunState.Rng.CombatTargets.NextItem(enemies);
                if (target == null)
                    return false;

                Flash();
                var results = (await dealDamage(target)).ToList();
                if (results.Count == 0)
                    return false;

                attackContext.AddHit(results);
                return true;
            }
            catch (OperationCanceledException) { }
            catch (Exception e)
            {
                MegaCrit.Sts2.Core.Logging.Log.Error($"[BloomStance] RunBloom Error: {e}");
            }
            finally
            {
                _isExecutingBloom = false;
            }
            return false;
        }

        private static decimal GetBloomDamageValue(CardModel cardModel, Creature? target)
        {
            if (cardModel.DynamicVars.ContainsKey("CalculatedDamage"))
            {
                return cardModel.DynamicVars.CalculatedDamage.Calculate(target);
            }

            return cardModel.DynamicVars.Damage.BaseValue;
        }

        private static decimal GetBloomModifiedDamage(CardModel cardModel, CardPlay? cardPlay, Creature target, decimal baseDamage)
        {
            if (cardModel.Owner?.RunState == null || cardModel.CombatState == null || cardModel.Owner.Creature == null)
            {
                return baseDamage;
            }

#if STS2_BETA
            return Hook.ModifyDamage(
                cardModel.Owner.RunState,
                cardModel.CombatState,
                target,
                cardModel.Owner.Creature,
                baseDamage,
                ValueProp.Move,
                cardModel,
                cardPlay,
                ModifyDamageHookType.Additive | ModifyDamageHookType.Multiplicative,
                CardPreviewMode.None,
                out _);
#else
            return Hook.ModifyDamage(
                cardModel.Owner.RunState,
                cardModel.CombatState,
                target,
                cardModel.Owner.Creature,
                baseDamage,
                ValueProp.Move,
                cardModel,
                ModifyDamageHookType.Additive | ModifyDamageHookType.Multiplicative,
                CardPreviewMode.None,
                out _);
#endif
        }

    }
}
