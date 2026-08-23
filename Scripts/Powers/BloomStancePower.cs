using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using HarmonyLib;
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

        
        private CardModel? _cardToIgnore;
        private AttackCommand? _commandToDouble;

        
        private bool _isExecutingBloom = false;

        private static readonly AccessTools.FieldRef<AttackCommand, Creature?> SingleTargetRef =
            AccessTools.FieldRefAccess<AttackCommand, Creature?>("_singleTarget");

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
            if (target == base.Owner) return 1m - (BlockReduction / 100m);
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


                var powerInstance = player.Creature.GetPower<BloomStancePower>();
                if (powerInstance != null)
                    powerInstance._cardToIgnore = sourceCard;

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
                MegaCrit.Sts2.Core.Logging.Log.Info("[BloomStance] BeforeAttack: blocked by isExecutingBloom");
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


            if (cardModel == _cardToIgnore)
            {
                _cardToIgnore = null;
                return Task.CompletedTask;
            }

            if (_commandToDouble != null)
                return Task.CompletedTask;

            _commandToDouble = command;
            MegaCrit.Sts2.Core.Logging.Log.Info($"[BloomStance] BeforeAttack: registered command for {cardModel.Id}");
            return Task.CompletedTask;
        }

        public override async Task AfterAttack(PlayerChoiceContext choiceContext, AttackCommand command)
        {
            if (command != _commandToDouble) return;

            _commandToDouble = null;
            MegaCrit.Sts2.Core.Logging.Log.Info("[BloomStance] AfterAttack: launching bloom attacks");
            await RunBloomAttacksAsync(choiceContext, command);
        }

       
        private async Task RunBloomAttacksAsync(PlayerChoiceContext choiceContext, AttackCommand originalCommand)
        {
            _isExecutingBloom = true;
            try
            {
                if (!IsCombatActive()) return;
                if (originalCommand.ModelSource is not CardModel cardModel) return;
                if (base.CombatState == null) return;

#if STS2_BETA
                AttackContext attackContext =
                    await AttackCommand.CreateContextAsync(base.CombatState, choiceContext, originalCommand.CardPlay!);
#else
                AttackContext attackContext =
                    await AttackCommand.CreateContextAsync(base.CombatState, choiceContext, cardModel);
#endif

                try
                {
                    int repeat = GetBloomRepeatCount(cardModel);

                    MegaCrit.Sts2.Core.Logging.Log.Info(
                        $"[BloomStance] RunBloom: repeat={repeat}");

                    this.Flash();

                    var bloomContext = new BlockingPlayerChoiceContext();

                    var opponents = base.CombatState.GetOpponentsOf(base.Owner);

                    for (int i = 0; i < repeat; i++)
                    {
                        if (!IsCombatActive()) break;

                        var validEnemies = opponents
                            .Where(e => e is { IsDead: false })
                            .ToList();

                        if (validEnemies.Count == 0)
                        {
                            MegaCrit.Sts2.Core.Logging.Log.Info("[BloomStance] RunBloom: no valid enemies");
                            break;
                        }

                        var randomTarget = base.Owner.Player?
                            .RunState?.Rng?.CombatTargets?.NextItem(validEnemies);

                        if (randomTarget == null) continue;

                        decimal dmgValue = GetBloomDamageValue(cardModel, randomTarget);
                        decimal modifiedDamage = GetBloomModifiedDamage(cardModel, originalCommand, randomTarget, dmgValue);

                        MegaCrit.Sts2.Core.Logging.Log.Info(
                            $"[BloomStance] RunBloom: hit {i + 1}/{repeat} 鈫?{randomTarget.GetType().Name} dmg={modifiedDamage}");

#if STS2_BETA
                        var results = await CreatureCmd.Damage(
                            bloomContext,
                            randomTarget,
                            modifiedDamage,
                            ValueProp.Unpowered,
                            base.Owner,
                            null,
                            null
                        );
#else
                        var results = await CreatureCmd.Damage(
                            bloomContext,
                            randomTarget,
                            modifiedDamage,
                            ValueProp.Unpowered,
                            base.Owner
                        );
#endif

                        attackContext.AddHit(results);

                        if (!IsCombatActive()) break;
                    }
                }
                finally
                {
                    await attackContext.DisposeAsync();
                }
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
        }

       
        private bool IsCombatActive()
        {
            var mgr = CombatManager.Instance;
            return mgr != null && mgr.IsInProgress && base.CombatState != null;
        }

        private static Creature? GetSingleTarget(AttackCommand command)
        {
            try
            {
                return SingleTargetRef(command);
            }
            catch
            {
                return null;
            }
        }

        private static decimal GetBloomDamageValue(CardModel cardModel, Creature? target)
        {
            if (cardModel.DynamicVars.ContainsKey("CalculatedDamage"))
            {
                return cardModel.DynamicVars.CalculatedDamage.Calculate(target);
            }

            return cardModel.DynamicVars.Damage.BaseValue;
        }

        private static decimal GetBloomModifiedDamage(CardModel cardModel, AttackCommand originalCommand, Creature target, decimal baseDamage)
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
                originalCommand.CardPlay,
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

        private static int GetBloomRepeatCount(CardModel cardModel)
        {
            int repeat = 1;

            if (cardModel.DynamicVars.ContainsKey("Repeat"))
            {
                repeat = Math.Max(1, cardModel.DynamicVars["Repeat"].IntValue);
            }
            else if (cardModel.DynamicVars.ContainsKey("Hits"))
            {
                repeat = Math.Max(1, cardModel.DynamicVars["Hits"].IntValue);
            }

            return repeat;
        }
    }
}
