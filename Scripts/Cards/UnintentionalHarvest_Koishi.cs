using System.Collections.Generic;
using System.Threading.Tasks;
using BaseLib.Abstracts;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.HoverTips;
using KomeijiKoishi.Pools;
using KomeijiKoishi.Enums;
using System;
using System.Linq;
using BaseLib.Utils;
using MegaCrit.Sts2.Core.Entities.Players; 
using MegaCrit.Sts2.Core.Models.Cards;
using MegaCrit.Sts2.Core.ValueProps;
using MegaCrit.Sts2.Core.Models.Powers; 

namespace KomeijiKoishi.Cards
{
    [Pool(typeof(KoishiCardPool))]
    public sealed class UnintentionalHarvest_Koishi : CustomCardModel,IUseAncientCardFace
    {
        public UnintentionalHarvest_Koishi() 
            : base(2, CardType.Skill, CardRarity.Rare, TargetType.Self, true)
        {
        }

        public override string PortraitPath => KoishiImagePaths.CardPortrait(GetType());

        protected override HashSet<CardTag> CanonicalTags => new HashSet<CardTag> { KoishiTags.Unconscious };

        public override IEnumerable<CardKeyword> CanonicalKeywords => new[] { KoishiKeywords.Unconscious };

        protected override IEnumerable<IHoverTip> ExtraHoverTips
        {
            get
            {
                yield return HoverTipFactory.FromKeyword(KoishiKeywords.Unconscious);
                if (ShouldRetainHand)
                {
                    yield return HoverTipFactory.FromKeyword(CardKeyword.Retain);
                }
            }
        }

        private bool ShouldRetainHand => !KomeijiKoishi.Config.KoishiBalanceManager.IsEnabled || base.IsUpgraded;

        protected override IEnumerable<DynamicVar> CanonicalVars => new List<DynamicVar> 
        { 
            new CardsVar(7) 
        };

        protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
        {
            var player = base.Owner as MegaCrit.Sts2.Core.Entities.Players.Player;
            if (player == null) return;

            await CreatureCmd.TriggerAnim(player.Creature, "Cast", player.Character!.CastAnimDelay);

            await CardPileCmd.Draw(choiceContext, base.DynamicVars.Cards.BaseValue, player, false);
            
            if (ShouldRetainHand)
            {
                await PowerCmd.Apply<RetainHandPower>(choiceContext,player.Creature, 1m, player.Creature, this, false);
            }
        }

        protected override void OnUpgrade()
        {
            if (!KomeijiKoishi.Config.KoishiBalanceManager.IsEnabled)
            {
                base.DynamicVars.Cards.UpgradeValueBy(2m);
            }
        }
    }
}
