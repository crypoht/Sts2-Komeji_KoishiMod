using System.Collections.Generic;
using System.Threading.Tasks;
using System; 
using BaseLib.Utils;
using BaseLib.Abstracts; 
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Models;  
using MegaCrit.Sts2.Core.Models.Cards;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.ValueProps;
using KomeijiKoishi.Pools; 
using KomeijiKoishi.Enums; 

namespace KomeijiKoishi.Cards
{
    [Pool(typeof(KoishiCardPool))]
    public sealed class InstinctiveStab_Koishi : CustomCardModel 
    {
        private const int energyCost = 2; 
        private const CardType type = CardType.Attack; 
        private const CardRarity rarity = CardRarity.Basic; 
        private const TargetType targetType = TargetType.AnyEnemy; 
        private const bool shouldShowInCardLibrary = true;

        public InstinctiveStab_Koishi() 
            : base(energyCost, type, rarity, targetType, shouldShowInCardLibrary)
        {
        }

        public override string PortraitPath => KoishiImagePaths.CardPortrait(GetType());

        protected override HashSet<CardTag> CanonicalTags => new HashSet<CardTag> { KoishiTags.Unconscious };

        public override IEnumerable<CardKeyword> CanonicalKeywords => new[] { KoishiKeywords.Unconscious };

        protected override IEnumerable<DynamicVar> CanonicalVars => new List<DynamicVar> 
        { 
            new DamageVar(KomeijiKoishi.Config.KoishiBalanceManager.Value(2m, 4m), ValueProp.Move),
            new RepeatVar(KomeijiKoishi.Config.KoishiBalanceManager.Value(6, 3))
        };

        protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
        {
            if (cardPlay.Target == null) return;

            await DamageCmd.Attack(base.DynamicVars.Damage.BaseValue)
                .WithHitCount(base.DynamicVars.Repeat.IntValue) 
                .FromCard(this, cardPlay)
                .Targeting(cardPlay.Target)
                .Execute(choiceContext);
        }

        protected override void OnUpgrade()
        {
            base.DynamicVars.Repeat.UpgradeValueBy(KomeijiKoishi.Config.KoishiBalanceManager.Value(3m, 1m));
        }
    }
}
