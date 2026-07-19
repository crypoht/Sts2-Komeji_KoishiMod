using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BaseLib.Abstracts;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Cards;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.ValueProps;
using KomeijiKoishi.Pools;
using KomeijiKoishi.Utils_Koishi;
using KomeijiKoishi.Enums;
using MegaCrit.Sts2.Core.HoverTips;
using BaseLib.Utils;

namespace KomeijiKoishi.Cards
{
    [Pool(typeof(KoishiCardPool))]
    public sealed class RoseRhapsodyPunch_Koishi : CustomCardModel
    {
        public RoseRhapsodyPunch_Koishi() 
            : base(2, CardType.Attack, CardRarity.Uncommon, TargetType.AnyEnemy, true) 
        { 
        }

        public override string PortraitPath => KoishiImagePaths.CardPortrait(GetType());

        protected override IEnumerable<DynamicVar> CanonicalVars => new List<DynamicVar> 
        { 
            new CalculationBaseVar(14m),
            new ExtraDamageVar(KomeijiKoishi.Config.KoishiBalanceManager.Value(3m, 2m)),
            new CalculatedDamageVar(ValueProp.Move).WithMultiplier(GetThornsAmount)
        };

        protected override IEnumerable<IHoverTip> ExtraHoverTips => new[] 
        { 
            HoverTipFactory.FromPower<ThornsPower>() 
        };

        protected override bool ShouldGlowGoldInternal => GetThornsAmount(this, null) > 0;

        protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
        {
            if (cardPlay.Target == null) return;

            await DamageCmd.Attack(base.DynamicVars.CalculatedDamage)
                .FromCard(this, cardPlay)
                .Targeting(cardPlay.Target)
                .WithHitFx("vfx/vfx_attack_blunt") 
                .Execute(choiceContext);
        }

        private static decimal GetThornsAmount(CardModel card, MegaCrit.Sts2.Core.Entities.Creatures.Creature? _)
        {
            return card.Owner?.Creature.GetPower<ThornsPower>()?.Amount ?? 0m;
        }

        protected override void OnUpgrade()
        {
            base.DynamicVars.ExtraDamage.UpgradeValueBy(1m);
        }
    }
}
