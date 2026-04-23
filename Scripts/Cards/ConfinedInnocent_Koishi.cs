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
using MegaCrit.Sts2.Core.HoverTips;

namespace KomeijiKoishi.Cards
{
    [Pool(typeof(KoishiCardPool))]

    public sealed class ConfinedInnocent_Koishi : CustomCardModel, IStanceListenerCard
    {

        public ConfinedInnocent_Koishi() 
            : base(2, CardType.Skill, CardRarity.Uncommon, TargetType.Self, true) 
        { 
        }

        public override string PortraitPath => $"res://mods/Komeiji_Koishi/images/cards/{GetType().Name}.png";

        protected override IEnumerable<DynamicVar> CanonicalVars => new List<DynamicVar> 
        { 
            new BlockVar(4m, ValueProp.Move),                 
            new DynamicVar("Magic", 3m)      
        };

        protected override IEnumerable<IHoverTip> ExtraHoverTips => new[] 
        { 
            HoverTipFactory.FromKeyword(KoishiKeywords.Stance) 
        };

        public Task OnStanceChanged(bool isClosedStance, bool isBloomStance)
        {

            base.AssertMutable();
            
            base.DynamicVars.Block.BaseValue += base.DynamicVars["Magic"].BaseValue;
             

            return Task.CompletedTask;
        }

        protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
        {
            var player = base.Owner as Player;
            if (player == null) return;

            await CreatureCmd.TriggerAnim(player.Creature, "Defend", player.Character!.CastAnimDelay);

           await CreatureCmd.GainBlock(player.Creature, base.DynamicVars.Block.BaseValue, ValueProp.Move, cardPlay, false);
        }

        protected override void OnUpgrade()
        {

            base.DynamicVars["Magic"].UpgradeValueBy(1m);
        }
    }
}