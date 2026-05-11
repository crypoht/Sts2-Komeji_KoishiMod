using System.Linq;
using MegaCrit.Sts2.Core.Models.Cards;
using KomeijiKoishi.Enums;  
using KomeijiKoishi.Powers; 
using System.Reflection; 
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Entities.Players; 
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using System.Collections.Generic;
using System;
using System.Linq;
using System.Threading.Tasks;
using BaseLib.Utils;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;


namespace KomeijiKoishi.Utils_Koishi
{
    public static class KoishiExtensions
    {

        public static bool IsTrulyUnconscious(CardModel card) //判断是否有无意识标签,返回值为bool
        {
            if (card == null) return false;


            if (card.Tags != null && card.Tags.Contains(KoishiTags.Unconscious)) 
            {
                return true;
            }


            if (card.CanonicalKeywords != null && card.CanonicalKeywords.Contains(KoishiKeywords.Unconscious)) 
            {
                return true;
            }


            if (card.Keywords != null && card.Keywords.Contains(KoishiKeywords.Unconscious))
            {
                return true;
            }

            if (card.Owner?.Creature != null)
            {
                bool hasFetusDream = card.Owner.Creature.Powers.Any(p => p is FetusDreamPower);

                if (hasFetusDream && card.Tags != null && card.Tags.Contains(KoishiTags.Subconscious)) 
                {
                    return true;
                }
            }

            return false;
        }

        public static void ApplyUnconsciousToCard(CardModel card)  //添加无意识标签的方法
        {
            if (card == null) return;

            CardCmd.ApplyKeyword(card, new CardKeyword[] { KoishiKeywords.Unconscious });
        }

        public static HashSet<CardModel> AutoPlayedByUnconsciousCards = new HashSet<CardModel>(); //这个不用管

         public static class UnconsciousPlayHelper  //这是一个优化后的自动打出,可以精准的识别目标(没有使用,因为发现没必要)
       {
        public static async Task SafeAutoPlayCard(PlayerChoiceContext choiceContext, Player player, CardModel targetCard)
        {
            if (targetCard == null || player.Creature.CombatState == null) return;

            try
            {
                Creature? targetCreature = null;
                var combatState = player.Creature.CombatState;


                switch (targetCard.TargetType)
                {
                    case TargetType.AnyEnemy:
                        var validEnemies = combatState.HittableEnemies.Where(e => !e.IsDead).ToList();
                        if (validEnemies.Count > 0)
                        {
                            targetCreature = player.RunState.Rng.Shuffle.NextItem(validEnemies);
                        }
                        break;
                    
                    case TargetType.Self:
                        targetCreature = player.Creature;
                        break;

                    case TargetType.AnyAlly:
                        var validAllies = combatState.GetTeammatesOf(player.Creature).Where(c => !c.IsDead).ToList();
                        if (validAllies.Count > 0)
                        {
                            targetCreature = player.RunState.Rng.Shuffle.NextItem(validAllies);
                        }
                        else
                        {
                            targetCreature = player.Creature; 
                        }
                        break;

                    case TargetType.AllAllies:
                    case TargetType.AllEnemies:
                    case TargetType.None:
                        targetCreature = null; 
                        break;
                }

                KoishiExtensions.AutoPlayedByUnconsciousCards.Add(targetCard);

                await CardCmd.AutoPlay(choiceContext, targetCard, targetCreature, AutoPlayType.Default, true, false);
            }
            catch (Exception e)
            {
                MegaCrit.Sts2.Core.Logging.Log.Error($"[UnconsciousPlayHelper] 自动打出卡牌失败: {targetCard.Id.Entry} | Error: {e}");
            }
            finally
            {
                KoishiExtensions.AutoPlayedByUnconsciousCards.Remove(targetCard);
            }
        }
    }
    }
}