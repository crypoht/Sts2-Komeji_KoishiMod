using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using BaseLib.Abstracts;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.Cards;
using MegaCrit.Sts2.Core.Models.Powers; 
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Nodes.Vfx;
using KomeijiKoishi.Pools;
using KomeijiKoishi.Powers;
using BaseLib.Utils;

namespace KomeijiKoishi.Cards
{
    [Pool(typeof(KoishiCardPool))]
    public sealed class BramblyRoseGarden_Koishi : CustomCardModel
    {
        public BramblyRoseGarden_Koishi()
            : base(1, CardType.Power, CardRarity.Uncommon, TargetType.Self, true)
        {
        }

        public override string PortraitPath => $"res://mods/Komeiji_Koishi/images/cards/{GetType().Name}.png";

        protected override IEnumerable<DynamicVar> CanonicalVars => new List<DynamicVar> 
        { 
            new PowerVar<ThornsPower>(1m) 
        };

        protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
        {
            var player = base.Owner as Player;
            if (player == null) return;

            NPowerUpVfx.CreateNormal(player.Creature);
            await CreatureCmd.TriggerAnim(player.Creature, "Cast", player.Character!.CastAnimDelay);
            
            decimal thornsInitial = base.DynamicVars["ThornsPower"].BaseValue;
            await PowerCmd.Apply<ThornsPower>(player.Creature, thornsInitial, player.Creature, this, false);

            await PowerCmd.Apply<BramblyRoseGardenPower>(player.Creature, 1m, player.Creature, this, false);
        }

        protected override void OnUpgrade()
        {

            base.DynamicVars["ThornsPower"].UpgradeValueBy(1m);
        }
    }
}