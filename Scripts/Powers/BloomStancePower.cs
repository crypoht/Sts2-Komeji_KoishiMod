using System;
using System.Linq;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Commands.Builders;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players; 
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.ValueProps;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;

namespace KomeijiKoishi.Powers
{
    public sealed class BloomStancePower : KoishiStancePower
    {
        public override string? CustomPackedIconPath => $"res://mods/Komeiji_Koishi/images/powers/BloomStancePower.png";
        public override string? CustomBigIconPath => "res://mods/Komeiji_Koishi/images/powers/BloomStancePower.png";
        public static int BloomEnergyGainAmount = 1;

        // 🌟 1. 官方合规的内部数据
        private class Data
        {
            public CardModel? cardToIgnore;
            public bool isExecuting;
        }

        protected override object InitInternalData() => new Data();

        protected override IEnumerable<IHoverTip> ExtraHoverTips => new[] 
        { 
            HoverTipFactory.ForEnergy(this) 
        };

        protected override IEnumerable<DynamicVar> CanonicalVars => new List<DynamicVar> 
        { 
            new DynamicVar("BlockReduction", 70m) 
        };

        public decimal BlockReduction => 70m;

        public override decimal ModifyBlockMultiplicative(Creature target, decimal block, ValueProp props, CardModel? cardSource, CardPlay? cardPlay)
        {
            if (target == base.Owner) return 1m - (BlockReduction / 100m); 
            return 1m; 
        }

        public static async Task EnterThisStance(PlayerChoiceContext context, Player player, CardModel sourceCard)
        {
            try 
            {
                if (player.Creature.GetPower<BloomStancePower>() != null) return;

                await ClearOldStances(player); 
                
                int bonusEnergy = 0;
                var superego = player.Creature.Powers.FirstOrDefault(p => p is SuperegoPower);
                if (superego != null) bonusEnergy = (int)superego.Amount;

                int totalEnergyGain = BloomEnergyGainAmount + bonusEnergy;
                if (totalEnergyGain > 0) await PlayerCmd.GainEnergy(totalEnergyGain, player);
                
                await PowerCmd.Apply<BloomStancePower>(player.Creature, 1m, player.Creature, sourceCard, false);
                
                var powerInstance = player.Creature.GetPower<BloomStancePower>();
                if (powerInstance != null)
                {
                    // 记录源卡牌，防止无限自循环
                    powerInstance.GetInternalData<Data>().cardToIgnore = sourceCard;
                }
                
                await NotifyAllCardsStanceChanged(player, "Bloom"); 
                await NotifyAllPowersStanceChanged(context, player, "Bloom", sourceCard);
            }
            catch (Exception e)
            {
                MegaCrit.Sts2.Core.Logging.Log.Error($"[BloomStance] Enter Error: {e}");
            }
        }

        // ========================================================
        // 🌟 2. 改造钩子：瞬间退出，绝不挂起！
        // ========================================================
        public override Task AfterCardPlayed(PlayerChoiceContext context, CardPlay cardPlay)
        {
            if (cardPlay?.Card == null || cardPlay.Card.Type != CardType.Attack || cardPlay.Card.Owner != base.Owner.Player) 
                return Task.CompletedTask; // 瞬间退出

            Data data = GetInternalData<Data>();

            if (cardPlay.Card == data.cardToIgnore)
            {
                data.cardToIgnore = null;
                return Task.CompletedTask; // 瞬间退出
            }

            if (data.isExecuting) return Task.CompletedTask;

            // 🌟 核心魔法：启动后台影子任务打伤害，用 _ = 告诉编译器不需要等待它！
            _ = RunBloomAttacksAsync(context, cardPlay, data);

            // 🌟 绝杀：直接返回完成！引擎会立刻安全地 Pop 掉盛开和卡牌，绝不卡死！
            return Task.CompletedTask; 
        }

        // ========================================================
        // 🌟 3. 后台影子任务：脱离了堆栈束缚，想怎么打怎么打
        // ========================================================
        // ========================================================
        // 🌟 3. 后台影子任务：完全消灭 CS8602 空引用警告版
        // ========================================================
        private async Task RunBloomAttacksAsync(PlayerChoiceContext context, CardPlay cardPlay, Data data)
        {
            data.isExecuting = true;
            try 
            {
                // 提前安全拦截
                if (base.CombatState == null || cardPlay.Card == null) return;

                AttackContext attackContext = await AttackCommand.CreateContextAsync(base.CombatState, cardPlay.Card); 

                try
                {
                    decimal dmgValue = cardPlay.Card.DynamicVars.Damage.BaseValue;
                    int repeat = 1;
                    try { repeat = (int)cardPlay.Card.DynamicVars["Repeat"].BaseValue; } catch { repeat = 1; }

                    this.Flash();

                    for (int i = 0; i < repeat; i++) 
                    {
                        // 🌟 防线 1：加入对 CombatManager.Instance 的 null 检查
                        if (MegaCrit.Sts2.Core.Combat.CombatManager.Instance == null || !MegaCrit.Sts2.Core.Combat.CombatManager.Instance.IsInProgress || base.CombatState == null) break;
                        
                        // 🌟 防线 2：加入对 HittableEnemies 的 ? 检查
                        var validEnemies = base.CombatState.HittableEnemies?.Where(e => e != null && !e.IsDead).ToList();
                        if (validEnemies == null || validEnemies.Count == 0) break; 
                        
                        // 🌟 防线 3：加入对 Player, RunState, Rng 的连环 ? 检查
                        var randomTarget = base.Owner.Player?.RunState?.Rng?.Shuffle?.NextItem(validEnemies);
                        
                        if (randomTarget != null)
                        {
                            var results = await CreatureCmd.Damage(
                                context,                
                                randomTarget,          
                                dmgValue,             
                                ValueProp.Unpowered,    
                                base.Owner,             
                                null                    
                            );
                            attackContext.AddHit(results);
                        }
                    }
                }
                finally
                {
                    if (attackContext != null) await attackContext.DisposeAsync();
                }
            }
            catch (OperationCanceledException) 
            { 
                // 战斗结束取消，安全吞噬
            } 
            catch (Exception e)
            {
                MegaCrit.Sts2.Core.Logging.Log.Error($"[BloomStance] Async Error: {e}");
            }
            finally
            {
                // 🌟 防线 4：finally 里的全面 ? 检查
                var aliveEnemies = base.CombatState?.HittableEnemies?.Where(e => e != null && !e.IsDead).ToList();
                if (aliveEnemies == null || aliveEnemies.Count == 0 || MegaCrit.Sts2.Core.Combat.CombatManager.Instance == null || !MegaCrit.Sts2.Core.Combat.CombatManager.Instance.IsInProgress)
                {
                    await Cmd.Wait(0.1f, true); 
                }

                data.isExecuting = false;
            }
        }
    }
}