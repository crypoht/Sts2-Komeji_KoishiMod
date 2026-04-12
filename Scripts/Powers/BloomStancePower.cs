using System;
using System.Linq;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players; 
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.ValueProps;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;

namespace KomeijiKoishi.Powers
{
    public sealed class BloomStancePower : KoishiStancePower
    {
        public override string? CustomPackedIconPath => $"res://mods/Komeiji_Koishi/images/powers/BloomStancePower.png";
        public static int BloomEnergyGainAmount = 1;
        private bool _isFirstCard = true;

        protected override IEnumerable<IHoverTip> ExtraHoverTips => new[] 
        { 
            HoverTipFactory.ForEnergy(this) 
        };

        public static async Task EnterThisStance(PlayerChoiceContext context, Player player, CardModel sourceCard)
        {
            if (player.Creature.Powers.Any(p => p is BloomStancePower)) {
                return;
            }
            ClearOldStances(player); 
            
            int bonusEnergy = 0;
            var superego = player.Creature.Powers.FirstOrDefault(p => p is SuperegoPower);
            if (superego != null)
            {
                bonusEnergy = (int)superego.Amount;
            }

            int totalEnergyGain = BloomEnergyGainAmount + bonusEnergy;

            if (totalEnergyGain > 0)
            {
                await PlayerCmd.GainEnergy(totalEnergyGain, player);
            }
            
            await PowerCmd.Apply<BloomStancePower>(player.Creature, 1m, player.Creature, sourceCard, false);
            NotifyAllCardsStanceChanged(player, "Bloom"); 
        }

        public override decimal ModifyBlockMultiplicative(Creature target, decimal block, ValueProp props, CardModel? cardSource, CardPlay? cardPlay)
        {
            if (target == base.Owner)
            {
                return 0.5m; 
            }
            
            return 1m; 
        }

        public override async Task AfterCardPlayed(PlayerChoiceContext context, CardPlay cardPlay)
        {
            try 
            {
                if (_isFirstCard)
                {
                    _isFirstCard = false;
                    return;
                }

                if (cardPlay?.Card != null && cardPlay.Card.Type == CardType.Attack && cardPlay.Card.Owner == base.Owner.Player)
                {
                    this.Flash();
                    
                    int repeat = 1;
                    
                    try 
                    {
                        repeat = (int)cardPlay.Card.DynamicVars["Repeat"].BaseValue;
                    }
                    catch 
                    {
                        repeat = 1;
                    }
                    
                    decimal dmgValue = cardPlay.Card.DynamicVars.Damage.BaseValue;

                    for (int i = 0; i < repeat; i++) 
                    {
                        var validEnemies = base.CombatState?.HittableEnemies.Where(e => !e.IsDead).ToList();
                        if (validEnemies == null || validEnemies.Count == 0) break;
                        
                        var randomTarget = base.Owner.Player.RunState.Rng.Shuffle.NextItem(validEnemies);
                        
                        if (randomTarget != null)
                        {
                            await DamageCmd.Attack(dmgValue)
                                .FromCard(cardPlay.Card)
                                .Targeting(randomTarget)
                                .WithHitFx("vfx/vfx_attack_slash")
                                .Execute(context);
                        }
                    }
                }
            }
            catch (Exception e)
            {
                MegaCrit.Sts2.Core.Logging.Log.Error($"[BloomStance] Softlock Prevented: {e}");
            }
        }
    }
}