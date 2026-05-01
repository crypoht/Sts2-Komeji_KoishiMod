using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using BaseLib.Abstracts;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.Cards;
using MegaCrit.Sts2.Core.ValueProps;
using KomeijiKoishi.Pools;
using KomeijiKoishi.Powers;
using BaseLib.Utils;
using KomeijiKoishi.Enums;

namespace KomeijiKoishi.Cards
{
    [Pool(typeof(KoishiCardPool))]
    public sealed class BoundaryHeart_Koishi : CustomCardModel
    {

        public BoundaryHeart_Koishi() 
            : base(1, CardType.Skill, CardRarity.Uncommon, TargetType.Self, true)
        {
        }

        public override IEnumerable<CardKeyword> CanonicalKeywords => new[] { KoishiKeywords.Closed };

        public override string PortraitPath => $"res://mods/Komeiji_Koishi/images/cards/{GetType().Name}.png";

        protected override IEnumerable<DynamicVar> CanonicalVars => new List<DynamicVar> 
        { 

            new BlockVar(12m, ValueProp.Move)
        };

        protected override bool ShouldGlowGoldInternal
        {
            get
            {
                var player = base.Owner as Player;
                if (player == null) return false;
                
                return player.Creature.GetPower<ClosedStancePower>() != null;
            }
        }

        protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
        {
            var player = base.Owner as Player;
            if (player == null) return;

            await CreatureCmd.TriggerAnim(player.Creature, "Defend", player.Character!.CastAnimDelay);

            bool alreadyInClosed = player.Creature.GetPower<ClosedStancePower>() != null;

            await ClosedStancePower.EnterThisStance(choiceContext, player, this);

            if (alreadyInClosed)
            {
                await CreatureCmd.GainBlock(player.Creature, base.DynamicVars.Block.BaseValue, ValueProp.Move, cardPlay, false);
            }
        }

        protected override void OnUpgrade()
        {
            base.DynamicVars.Block.UpgradeValueBy(4m);
        }
    }
}