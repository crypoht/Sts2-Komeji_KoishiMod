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
using MegaCrit.Sts2.Core.HoverTips; 

namespace KomeijiKoishi.Cards
{
    [Pool(typeof(KoishiCardPool))]
    public sealed class VoidExpansion_Koishi : CustomCardModel
    {
        public VoidExpansion_Koishi() 
            : base(1, CardType.Skill, CardRarity.Uncommon, TargetType.Self, true)
        {
        }

        protected override IEnumerable<IHoverTip> ExtraHoverTips => new[] 
        { 
            HoverTipFactory.FromPower<BloomStancePower>() 
        };

        public override string PortraitPath => $"res://mods/Komeiji_Koishi/images/cards/{GetType().Name}.png";

        protected override IEnumerable<DynamicVar> CanonicalVars => new List<DynamicVar> 
        { 
            new DynamicVar("Magic", 3m) 
        };

        protected override bool ShouldGlowGoldInternal
        {
            get
            {
                var player = base.Owner as Player;
                if (player == null) return false;
                
                return player.Creature.GetPower<BloomStancePower>() != null;
            }
        }

        protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
        {
            var player = base.Owner as Player;
            if (player == null) return;

            await CreatureCmd.TriggerAnim(player.Creature, "Buff", player.Character!.CastAnimDelay);

            bool alreadyInBloom = player.Creature.GetPower<BloomStancePower>() != null;

            await BloomStancePower.EnterThisStance(choiceContext, player, this);

            if (alreadyInBloom)
            {
                decimal kuugaAmount = base.DynamicVars["Magic"].BaseValue;
                await PowerCmd.Apply<KuugaPower>(choiceContext,player.Creature, kuugaAmount, player.Creature, this, false);
            }
        }

        protected override void OnUpgrade()
        {
            base.DynamicVars["Magic"].UpgradeValueBy(1m);
        }
    }
}