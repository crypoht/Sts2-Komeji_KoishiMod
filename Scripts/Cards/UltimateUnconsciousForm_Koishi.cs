using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using BaseLib.Utils;     
using BaseLib.Abstracts; 
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer; 
using KomeijiKoishi.Pools; 
using KomeijiKoishi.Powers; 
using KomeijiKoishi.Utils_Koishi; 
using KomeijiKoishi.Enums;
using MegaCrit.Sts2.Core.HoverTips;

namespace KomeijiKoishi.Cards
{
    [Pool(typeof(KoishiCardPool))]
    public sealed class UltimateUnconsciousForm_Koishi : CustomCardModel
    {
        public UltimateUnconsciousForm_Koishi() : base(3, CardType.Power, CardRarity.Rare, TargetType.Self, true) { }

        public override string PortraitPath => $"res://mods/Komeiji_Koishi/images/cards/{GetType().Name}.png";
        protected override HashSet<CardTag> CanonicalTags => new HashSet<CardTag> { KoishiTags.Subconscious };

        protected override IEnumerable<IHoverTip> ExtraHoverTips => new[] 
        { 
            HoverTipFactory.FromKeyword(KoishiKeywords.Unconscious) 
        };
        protected override IEnumerable<DynamicVar> CanonicalVars => new List<DynamicVar> { new DynamicVar("FormAmount", 1m) };

        protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
        {
            var owner = KoishiExtensions.GetOwner(this);
            if (owner != null)
            {
                await CreatureCmd.TriggerAnim(owner.Creature, "Buff", owner.Character.CastAnimDelay);
                await PowerCmd.Apply<UltimateUnconsciousFormPower>(owner.Creature, base.DynamicVars["FormAmount"].BaseValue, owner.Creature, this, false);
            }
        }

        protected override void OnUpgrade() => base.DynamicVars["FormAmount"].UpgradeValueBy(1m);
    }
}