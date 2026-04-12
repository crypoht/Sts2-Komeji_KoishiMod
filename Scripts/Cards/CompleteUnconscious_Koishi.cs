using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using BaseLib.Utils;     
using BaseLib.Abstracts; 
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Cards;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer; 
using KomeijiKoishi.Pools; 
using KomeijiKoishi.Utils_Koishi; 
using KomeijiKoishi.Enums;

namespace KomeijiKoishi.Cards
{
    [Pool(typeof(KoishiCardPool))]
    public sealed class CompleteUnconscious_Koishi : CustomCardModel
    {
        public CompleteUnconscious_Koishi() : base(514, CardType.Skill, CardRarity.Uncommon, TargetType.Self, true) { }

        public override string PortraitPath => $"res://mods/Komeiji_Koishi/images/cards/{GetType().Name}.png";
        protected override HashSet<CardTag> CanonicalTags => new HashSet<CardTag> { KoishiTags.Subconscious };
        

        public override IEnumerable<CardKeyword> CanonicalKeywords
        {
            get
            {
                try { return KoishiExtensions.IsUnconscious(this) ? new[] { KoishiKeywords.Unconscious } : new CardKeyword[0]; }
                catch (Exception) { return new[] {CardKeyword.Ethereal}; }
            }
        }
        protected override IEnumerable<IHoverTip> ExtraHoverTips => new List<IHoverTip> { HoverTipFactory.FromPower<IntangiblePower>() };

        protected override IEnumerable<DynamicVar> CanonicalVars => new List<DynamicVar> { new PowerVar<IntangiblePower>(1m) };

        protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
        {
            var owner = KoishiExtensions.GetOwner(this);
            if (owner != null)
            {
                await CreatureCmd.TriggerAnim(owner.Creature, "Cast", owner.Character.CastAnimDelay);
                await PowerCmd.Apply<IntangiblePower>(owner.Creature, base.DynamicVars["IntangiblePower"].BaseValue, owner.Creature, this, false);
            }
        }

        protected override void OnUpgrade() => base.DynamicVars["IntangiblePower"].UpgradeValueBy(1m);
    }
}