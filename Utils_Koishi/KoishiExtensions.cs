using System.Linq;
using System.Reflection; 
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Cards;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Entities.Players; 
using KomeijiKoishi.Enums;  
using KomeijiKoishi.Powers; 

namespace KomeijiKoishi.Utils_Koishi
{
    public static class KoishiExtensions
    {
        public static Player? GetOwner(CardModel card)
        {
            var ownerProp = typeof(CardModel).GetProperty("Owner", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            if (ownerProp != null)
            {
                return ownerProp.GetValue(card) as Player;
            }
            return null;
        }

        public static bool IsUnconscious(CardModel card)
        {
            if (card.Tags.Contains(KoishiTags.Unconscious)) return true;

            if (card.Tags.Contains(KoishiTags.Subconscious))
            {
                var owner = GetOwner(card);
                return CombatManager.Instance.IsInProgress && 
                       owner != null && 
                       owner.Creature.HasPower<FetusDreamPower>();
            }
            return false;
        }

        public static bool IsTrulyUnconscious(CardModel c)
        {
            if (c == null) return false;

            bool isUnc = false;
            
            try 
            {
                isUnc = IsUnconscious(c); 
            } 
            catch { }
            
            if (!isUnc)
            {
                isUnc = c.Tags != null && c.Tags.Contains(KoishiTags.Unconscious);
            }

            if (c.Owner != null && c.Owner.Creature.Powers.Any(p => p is FetusDreamPower))
            {
                if (!string.IsNullOrEmpty(c.Title) && c.Title.Contains("无意识"))
                {
                    return true;
                }
            }

            return isUnc;
        }
    }
}