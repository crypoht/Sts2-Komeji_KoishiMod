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

        public static bool IsTrulyUnconscious(CardModel card)
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

        public static void ApplyUnconsciousToCard(CardModel card)
        {
            if (card == null) return;

            CardCmd.ApplyKeyword(card, new CardKeyword[] { KoishiKeywords.Unconscious });
        }

        public static HashSet<CardModel> AutoPlayedByUnconsciousCards = new HashSet<CardModel>();
    }
}