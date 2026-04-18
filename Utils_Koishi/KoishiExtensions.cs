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

namespace KomeijiKoishi.Utils_Koishi
{
    public static class KoishiExtensions
    {
        private static HashSet<CardModel> TempUnconsciousCardsThisCombat = new HashSet<CardModel>();
        
        private static CombatState? _currentCombatState = null;

        private static void AutoCleanCheck(CombatState? combatState)
        {
            if (combatState != null && _currentCombatState != combatState)
            {
                TempUnconsciousCardsThisCombat.Clear();
                _currentCombatState = combatState;
            }
        }

        public static bool IsTrulyUnconscious(CardModel card)
        {
            if (card == null) return false;

            AutoCleanCheck(card.CombatState);

            if (TempUnconsciousCardsThisCombat.Contains(card))
            {
                return true;
            }

            if (card.Tags != null && (int)KoishiTags.Unconscious != 0 && card.Tags.Contains(KoishiTags.Unconscious))
            {
                return true;
            }
            if (card.CanonicalKeywords != null && (int)KoishiKeywords.Unconscious != 0 && card.CanonicalKeywords.Contains(KoishiKeywords.Unconscious))
            {
                return true;
            }

            if (card.Owner != null && card.Owner.Creature != null)
            {
                bool hasFetusDream = card.Owner.Creature.Powers.Any(p => p is FetusDreamPower);

                if (hasFetusDream)
                {
                    string className = card.GetType().Name;
                    if (card.Tags != null && (int)KoishiTags.Subconscious != 0 && card.Tags.Contains(KoishiTags.Subconscious)) 
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        public static void ApplyUnconsciousToCard(CardModel card)
        {
            if (card == null) return;

            AutoCleanCheck(card.CombatState);

            CardCmd.ApplyKeyword(card, new CardKeyword[] { KoishiKeywords.Unconscious });

            if (!TempUnconsciousCardsThisCombat.Contains(card))
            {
                TempUnconsciousCardsThisCombat.Add(card);
            }
        }

        public static HashSet<CardModel> AutoPlayedByUnconsciousCards = new HashSet<CardModel>();
    }
}