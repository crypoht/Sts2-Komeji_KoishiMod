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

                if (cardPlay?.Card != null && cardPlay.Card.Owner == base.Owner.Player)
                {
                    if (KoishiExtensions.IsTrulyUnconscious(cardPlay.Card))
                    {
                        this.Flash();
                        await CardPileCmd.Draw(context, 1, base.Owner.Player, false); 
                    }

                    if (KoishiExtensions.AutoPlayedByUnconsciousCards.Contains(cardPlay.Card))
                    {
                        return; 
                    }

                    FormPowerData data = base.GetInternalData<FormPowerData>();
                    if (!data.isAutoPlaying)
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

                                    KoishiExtensions.AutoPlayedByUnconsciousCards.Add(targetCard);
                                    this.Flash();
                                    
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
                }
            } 
            catch (Exception e) 
            {
                MegaCrit.Sts2.Core.Logging.Log.Error($"[UltimateUnconsciousForm] Softlock Prevented: {e}");
            }
        }

        private class FormPowerData 
        { 
            public bool isAutoPlaying; 
        }
    }
}