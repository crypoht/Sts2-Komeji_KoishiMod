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
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.ValueProps;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;

namespace KomeijiKoishi.Powers
{
    public sealed class BloomStancePower : KoishiStancePower
    {
        public override string? CustomPackedIconPath =>
            "res://mods/Komeiji_Koishi/images/powers/BloomStancePower.png";
        public override string? CustomBigIconPath =>
            "res://mods/Komeiji_Koishi/images/powers/BloomStancePower.png";

        public static int BloomEnergyGainAmount = 1;

        // ──────────────────────────────────────────────────
        // 直接用实例字段存状态，避免 GetInternalData 多实例问题
        // ──────────────────────────────────────────────────
        private CardModel? _cardToIgnore;
        private AttackCommand? _commandToDouble;

        // 硬防重入：用实例字段而非 Data 类，保证 await gap 期间也能正确读到
        private bool _isExecutingBloom = false;

        protected override object InitInternalData() => new object();

        protected override IEnumerable<IHoverTip> ExtraHoverTips =>
            new[] { HoverTipFactory.ForEnergy(this) };

        protected override IEnumerable<DynamicVar> CanonicalVars =>
            new List<DynamicVar> { new DynamicVar("BlockReduction", 70m) };

        public decimal BlockReduction => 70m;

        // ──────────────────────────────────────────────────
        // 格挡减益
        // ──────────────────────────────────────────────────
        public override decimal ModifyBlockMultiplicative(
            Creature target, decimal block, ValueProp props,
            CardModel? cardSource, CardPlay? cardPlay)
        {
            if (target == base.Owner) return 1m - (BlockReduction / 100m);
            return 1m;
        }

        // ──────────────────────────────────────────────────
        // 进入盛开
        // ──────────────────────────────────────────────────
        public static async Task EnterThisStance(
            PlayerChoiceContext context, Player player, CardModel sourceCard)
        {
            try
            {
                if (player.Creature.GetPower<BloomStancePower>() != null) return;

                await ClearOldStances(player);

                int bonusEnergy = 0;
                var superego = player.Creature.Powers.FirstOrDefault(p => p is SuperegoPower);
                if (superego != null) bonusEnergy = (int)superego.Amount;

                int totalEnergyGain = BloomEnergyGainAmount + bonusEnergy;
                if (totalEnergyGain > 0)
                    await PlayerCmd.GainEnergy(totalEnergyGain, player);

                await PowerCmd.Apply<BloomStancePower>(
                    player.Creature, 1m, player.Creature, sourceCard, false);

                // 直接拿实例写 _cardToIgnore
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

        // ──────────────────────────────────────────────────
        // BeforeAttack
        // ──────────────────────────────────────────────────
        public override Task BeforeAttack(AttackCommand command)
        {
            // 硬防重入：盛开攻击执行期间屏蔽所有新登记
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

            // 跳过触发进入盛开的源卡牌（只跳过一次）
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

        // ──────────────────────────────────────────────────
        // AfterAttack
        // ──────────────────────────────────────────────────
        public override async Task AfterAttack(AttackCommand command)
        {
            if (command != _commandToDouble) return;

            _commandToDouble = null;
            MegaCrit.Sts2.Core.Logging.Log.Info("[BloomStance] AfterAttack: launching bloom attacks");
            await RunBloomAttacksAsync(command);
        }

        // ──────────────────────────────────────────────────
        // 盛开翻倍伤害逻辑
        // ──────────────────────────────────────────────────
        private async Task RunBloomAttacksAsync(AttackCommand originalCommand)
        {
            _isExecutingBloom = true;
            try
            {
                if (!IsCombatActive()) return;
                if (originalCommand.ModelSource is not CardModel cardModel) return;
                if (base.CombatState == null) return;

                AttackContext attackContext =
                    await AttackCommand.CreateContextAsync(base.CombatState, cardModel);

                try
                {
                    decimal dmgValue = cardModel.DynamicVars.Damage.BaseValue;

                    int repeat = 1;
                    try { repeat = (int)cardModel.DynamicVars["Repeat"].BaseValue; }
                    catch { }

                    MegaCrit.Sts2.Core.Logging.Log.Info(
                        $"[BloomStance] RunBloom: dmg={dmgValue} repeat={repeat}");

                    this.Flash();

                    var bloomContext = new BlockingPlayerChoiceContext();

                    // 用 GetOpponentsOf 而非 HittableEnemies，确保拿到正确的敌人列表
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

                        MegaCrit.Sts2.Core.Logging.Log.Info(
                            $"[BloomStance] RunBloom: hit {i + 1}/{repeat} → {randomTarget.GetType().Name} dmg={dmgValue}");

                        var results = await CreatureCmd.Damage(
                            bloomContext,
                            randomTarget,
                            dmgValue,
                            ValueProp.Unpowered,
                            base.Owner,
                            cardModel 
                        );

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

        // ──────────────────────────────────────────────────
        // 辅助
        // ──────────────────────────────────────────────────
        private bool IsCombatActive()
        {
            var mgr = CombatManager.Instance;
            return mgr != null && mgr.IsInProgress && base.CombatState != null;
        }
    }
}