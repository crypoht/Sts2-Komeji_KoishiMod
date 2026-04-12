using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BaseLib.Utils; 
using BaseLib.Abstracts; 
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures; 
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer; 
using KomeijiKoishi.Pools; 
using KomeijiKoishi.Enums;
using KomeijiKoishi.Powers; 

namespace KomeijiKoishi.Cards
{
    [Pool(typeof(KoishiCardPool))]
    public sealed class ReactionYoukaiPolygraph_Koishi : CustomCardModel
    {
        public ReactionYoukaiPolygraph_Koishi() 
            : base(1, CardType.Skill, CardRarity.Uncommon, TargetType.AnyEnemy, true) { }

        public override string PortraitPath => $"res://mods/Komeiji_Koishi/images/cards/{GetType().Name}.png";

        public override IEnumerable<CardKeyword> CanonicalKeywords => new[] { KoishiKeywords.Bloom };

        protected override bool ShouldGlowGoldInternal
        {
            get
            {
                if (base.CombatState == null) return false;
                
                return base.CombatState.HittableEnemies.Any(e => e.Monster != null && e.Monster.IntendsToAttack);
            }
        }

        protected override IEnumerable<DynamicVar> CanonicalVars => new List<DynamicVar> 
        { 
            new DynamicVar("Draw", 3m) 
        };

        protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
        {
            if (cardPlay.Target == null) return;

            var player = base.Owner as MegaCrit.Sts2.Core.Entities.Players.Player;
            if (player == null) return;

            await CreatureCmd.TriggerAnim(player.Creature, "Buff", player.Character!.CastAnimDelay);

            await BloomStancePower.EnterThisStance(choiceContext, player, this);

            if (cardPlay.Target.Monster != null && cardPlay.Target.Monster.IntendsToAttack)
            {
                await CardPileCmd.Draw(choiceContext, base.DynamicVars["Draw"].BaseValue, base.Owner, false);
            }
        }

        protected override void OnUpgrade()
        {
            base.DynamicVars["Draw"].UpgradeValueBy(1m);
        }
    }
}