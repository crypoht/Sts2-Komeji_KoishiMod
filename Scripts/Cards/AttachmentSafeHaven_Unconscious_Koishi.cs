using System.Collections.Generic;
using System.Threading.Tasks;
using BaseLib.Abstracts;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.ValueProps;
using KomeijiKoishi.Pools;
using KomeijiKoishi.Enums;
using System;
using System.Linq;
using BaseLib.Utils;
using MegaCrit.Sts2.Core.Models;  
 

namespace KomeijiKoishi.Cards
{
    [Pool(typeof(KoishiCardPool))]
    public sealed class AttachmentSafeHaven_Unconscious_Koishi : CustomCardModel
    {
        public AttachmentSafeHaven_Unconscious_Koishi() 
            : base(3, CardType.Skill, CardRarity.Uncommon, TargetType.Self, true) { }

        public override string PortraitPath => $"res://mods/Komeiji_Koishi/images/cards/{GetType().Name}.png";

        protected override HashSet<CardTag> CanonicalTags => new HashSet<CardTag> { KoishiTags.Unconscious };
        public override IEnumerable<CardKeyword> CanonicalKeywords => new[] { KoishiKeywords.Unconscious };

        protected override IEnumerable<DynamicVar> CanonicalVars => new List<DynamicVar> 
        { 
            new BlockVar(25m, ValueProp.Move) 
        };

        protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
        {
            var player = base.Owner as MegaCrit.Sts2.Core.Entities.Players.Player;
            if (player == null) return;

            await CreatureCmd.TriggerAnim(player.Creature, "Defend", player.Character!.CastAnimDelay);


await CreatureCmd.GainBlock(player.Creature, base.DynamicVars.Block.BaseValue, ValueProp.Move, cardPlay, false);
        }

        protected override void OnUpgrade()
        {
            base.DynamicVars.Block.UpgradeValueBy(8m);
        }
    }
}