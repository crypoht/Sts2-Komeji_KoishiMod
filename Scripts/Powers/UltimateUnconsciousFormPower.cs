using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BaseLib.Utils;
using BaseLib.Abstracts; 
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.Entities.Creatures; 
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using KomeijiKoishi.Enums;
using KomeijiKoishi.Utils_Koishi; 

namespace KomeijiKoishi.Powers
{
    public sealed class UltimateUnconsciousFormPower : CustomPowerModel
    {
        public override PowerType Type => PowerType.Buff;
        public override PowerStackType StackType => PowerStackType.Counter;
        public override string? CustomPackedIconPath => $"res://mods/Komeiji_Koishi/images/powers/UltimateUnconsciousFormPower.png";
        public override string? CustomBigIconPath => $"res://mods/Komeiji_Koishi/images/powers/UltimateUnconsciousFormPower.png";
        
        protected override object InitInternalData() => new FormPowerData();

        public override async Task AfterCardPlayed(PlayerChoiceContext context, CardPlay cardPlay)
        {
            try 
            {
                if (base.CombatState == null || base.CombatState.HittableEnemies.All(e => e.IsDead)) return;
                if (cardPlay?.Card == null || cardPlay.Card.Owner != base.Owner.Player) return;

                FormPowerData data = base.GetInternalData<FormPowerData>();

                if (KoishiExtensions.IsTrulyUnconscious(cardPlay.Card))
                {
                    data.pendingDraws++;
                }

                if (!KoishiExtensions.AutoPlayedByUnconsciousCards.Contains(cardPlay.Card) && !data.isAutoPlaying)
                {
                    try 
                    {
                        data.isAutoPlaying = true; 
                        var handPile = PileType.Hand.GetPile(base.Owner.Player);
                        
                        var unconsciousCards = handPile.Cards
                            .Where(c => KoishiExtensions.IsTrulyUnconscious(c) && 
                                        (c.Keywords == null || !c.Keywords.Contains(CardKeyword.Unplayable)) && 
                                        c != cardPlay.Card)
                            .ToList();

                        int cardsToPlay = Math.Min((int)base.Amount, unconsciousCards.Count);

                        for (int i = 0; i < cardsToPlay; i++)
                        {
                            if (base.CombatState.HittableEnemies.All(e => e.IsDead)) break; 

                            var targetCard = base.Owner.Player.RunState.Rng.Shuffle.NextItem(unconsciousCards);
                            if (targetCard != null)
                            {
                                unconsciousCards.Remove(targetCard); 
                                
                                Creature? targetCreature = null;
                                if (targetCard.TargetType == TargetType.AnyEnemy)
                                {
                                    var validEnemies = base.CombatState.HittableEnemies.Where(e => !e.IsDead).ToList();
                                    if (validEnemies.Count > 0)
                                    {
                                        targetCreature = base.Owner.Player.RunState.Rng.Shuffle.NextItem(validEnemies);
                                    }
                                }
                                else if (targetCard.TargetType == TargetType.Self)
                                {
                                    targetCreature = base.Owner;
                                }

                                KoishiExtensions.AutoPlayedByUnconsciousCards.Add(targetCard);
                                
                                await CardCmd.AutoPlay(context, targetCard, targetCreature, AutoPlayType.Default, true, false);
                                
                                KoishiExtensions.AutoPlayedByUnconsciousCards.Remove(targetCard);
                            }
                        }
                    }
                    finally
                    {
                        data.isAutoPlaying = false; 
                    }
                }

 
                if (!data.isAutoPlaying && data.pendingDraws > 0)
                {
                    int draws = data.pendingDraws;
                    data.pendingDraws = 0; 
                    
                    this.Flash(); 
                    await CardPileCmd.Draw(context, draws, base.Owner.Player, false); 
                }
            } 
            catch (Exception e) 
            {
                MegaCrit.Sts2.Core.Logging.Log.Error($"[UltimateUnconsciousForm] Error Prevented: {e}");
            }
        }


        private class FormPowerData 
        { 
            public bool isAutoPlaying; 
            public int pendingDraws = 0; 
        }
    }
}