using System.Collections.Generic;
using System.Threading.Tasks;
using BaseLib.Abstracts;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using KomeijiKoishi.Pools;
using KomeijiKoishi.Powers;
using System;
using BaseLib.Utils;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.ValueProps;
using KomeijiKoishi.Enums;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers; 

namespace KomeijiKoishi.Cards
{
    [Pool(typeof(KoishiCardPool))]
    public sealed class RoseProtection_Koishi : CustomCardModel
    {
        public RoseProtection_Koishi() 
            : base(1, CardType.Power, CardRarity.Uncommon, TargetType.Self, true) { }

        public override string PortraitPath => $"res://mods/Komeiji_Koishi/images/cards/{GetType().Name}.png";

        private class RoseVar : DynamicVar { public RoseVar(decimal val) : base("Rose", val) { } }

        protected override IEnumerable<DynamicVar> CanonicalVars => new List<DynamicVar> 
        { 
            new RoseVar(40m) 
        };

        protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
        {
            var player = base.Owner as MegaCrit.Sts2.Core.Entities.Players.Player;
            if (player == null) return;

            await CreatureCmd.TriggerAnim(player.Creature, "Buff", player.Character!.CastAnimDelay);

            decimal amount = base.DynamicVars["Rose"].BaseValue;

            await PowerCmd.Apply<RoseProtectionPower>(player.Creature, amount, player.Creature, this, false);
        }

        protected override void OnUpgrade()
        {
            base.DynamicVars["Rose"].UpgradeValueBy(10m);
        }
    }
}