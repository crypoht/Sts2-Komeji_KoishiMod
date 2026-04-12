using System; 
using System.Collections.Generic;
using System.Threading.Tasks;
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
    public sealed class KomeijiSpin_Koishi : CustomCardModel 
    {
        private const int energyCost = 2; 
        private const CardType type = CardType.Attack; 
        private const CardRarity rarity = CardRarity.Common; 
        private const TargetType targetType = TargetType.AllEnemies; 
        private const bool shouldShowInCardLibrary = true;

        public KomeijiSpin_Koishi() 
            : base(energyCost, type, rarity, targetType, shouldShowInCardLibrary)
        {
        }

        public override string PortraitPath => $"res://mods/Komeiji_Koishi/images/cards/{GetType().Name}.png";

        protected override IEnumerable<DynamicVar> CanonicalVars => new List<DynamicVar> 
        { 
            new DamageVar(6m, ValueProp.Move),
            new RepeatVar(2)
        };

        protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
        {
            await DamageCmd.Attack(base.DynamicVars.Damage.BaseValue)
                .WithHitCount(base.DynamicVars.Repeat.IntValue) 
                .FromCard(this)
                .TargetingAllOpponents(base.CombatState!)
                .Execute(choiceContext);
        }

        protected override void OnUpgrade()
        {
            base.DynamicVars.Repeat.UpgradeValueBy(1m);
        }
    }
}