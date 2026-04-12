using BaseLib.Abstracts;
using BaseLib.Patches.Content; 
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Models.Cards;

namespace KomeijiKoishi.Enums
{
    public class KoishiTags
    {
        [CustomEnum]
        public static CardTag Unconscious; 

        [CustomEnum]
        public static CardTag Subconscious; 

        [CustomEnum]
        public static CardTag Danmaku;
    }

    public class KoishiKeywords
    {
        [CustomEnum("UNCONSCIOUS")] 
        [KeywordProperties(AutoKeywordPosition.Before)] 
        public static CardKeyword Unconscious;

        [CustomEnum("BLOOM")] 
        public static CardKeyword Bloom;

        [CustomEnum("CLOSED")] 
        public static CardKeyword Closed;

        [CustomEnum("DANMAKU")] 
        public static CardKeyword Danmaku;
    }
}