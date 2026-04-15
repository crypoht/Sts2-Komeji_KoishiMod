using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using BaseLib.Abstracts;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Models.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using KomeijiKoishi.Pools;
using KomeijiKoishi.Enums;
using BaseLib.Utils;  
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models; 
using MegaCrit.Sts2.Core.Models.Powers; 
using MegaCrit.Sts2.Core.ValueProps;
using MegaCrit.Sts2.Core.Models.CardPools;

namespace KomeijiKoishi.Cards.Danmaku
{
    [Pool(typeof(TokenCardPool))]
    public sealed class ConsciousOverflow_Koishi : CustomCardModel
    {
        public ConsciousOverflow_Koishi() 
            : base(514, CardType.Skill, CardRarity.Token, TargetType.None, true) { }

        public override string PortraitPath => $"res://mods/Komeiji_Koishi/images/cards/{GetType().Name}.png";

        protected override HashSet<CardTag> CanonicalTags => new HashSet<CardTag> { KoishiTags.Unconscious };

        public override IEnumerable<CardKeyword> CanonicalKeywords => new[] { KoishiKeywords.Unconscious };

        protected override Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
        {
            return Task.CompletedTask;
        }

        protected override void OnUpgrade()
        {
        }
    }
}