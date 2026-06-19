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

        [CustomEnum]
        public static CardTag Stance;

        [CustomEnum]
        public static CardTag Fumo;

        [CustomEnum]
        public static CardTag LostInFlowerField;
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

        [CustomEnum("STANCE")] 
        public static CardKeyword Stance;

        [CustomEnum("FUMO")] 
        public static CardKeyword Fumo;

        [CustomEnum("LOST_IN_FLOWER_FIELD")] 
        [KeywordProperties(AutoKeywordPosition.Before)]
        public static CardKeyword LostInFlowerField;
    }
}