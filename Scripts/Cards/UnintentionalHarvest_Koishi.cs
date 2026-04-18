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



namespace KomeijiKoishi.Cards
{
    [Pool(typeof(KoishiCardPool))]
    public sealed class UnintentionalHarvest_Koishi : CustomCardModel
    {
        public UnintentionalHarvest_Koishi() 
            : base(4, CardType.Skill, CardRarity.Rare, TargetType.Self, true)
        {
        }

        public override string PortraitPath => $"res://mods/Komeiji_Koishi/images/cards/{GetType().Name}.png";

        protected override HashSet<CardTag> CanonicalTags => new HashSet<CardTag> { KoishiTags.Unconscious };

        public override IEnumerable<CardKeyword> CanonicalKeywords => new[] { KoishiKeywords.Unconscious };

        protected override IEnumerable<IHoverTip> ExtraHoverTips => new List<IHoverTip> 
        { 
            HoverTipFactory.FromKeyword(KoishiKeywords.Unconscious) 
        };

        protected override IEnumerable<DynamicVar> CanonicalVars => new List<DynamicVar> 
        { 
            new CardsVar(6) 
        };

        protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
        {
            var player = base.Owner as MegaCrit.Sts2.Core.Entities.Players.Player;
            if (player == null) return;

            await CreatureCmd.TriggerAnim(player.Creature, "Cast", player.Character.CastAnimDelay);

            await CardPileCmd.Draw(choiceContext, base.DynamicVars.Cards.BaseValue, player, false);
        }

        protected override void OnUpgrade()
        {
            base.DynamicVars.Cards.UpgradeValueBy(2m);
        }
    }
}