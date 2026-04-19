using System.Collections.Generic;
using System.Threading.Tasks;
using BaseLib.Abstracts;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Models.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using KomeijiKoishi.Pools;
using KomeijiKoishi.Powers;
using MegaCrit.Sts2.Core.Entities.Creatures;
using KomeijiKoishi.Enums; 
using System;
using BaseLib.Utils;    
using MegaCrit.Sts2.Core.Models; 
using KomeijiKoishi.Utils_Koishi; 


namespace KomeijiKoishi.Cards
{
    [Pool(typeof(KoishiCardPool))]
    public sealed class PhilosophyOfTheHated_Koishi : CustomCardModel
    {
        public PhilosophyOfTheHated_Koishi() 
            : base(1, CardType.Power, CardRarity.Uncommon, TargetType.Self, true) { }

        public override IEnumerable<CardKeyword> CanonicalKeywords => new[] { KoishiKeywords.Closed };

        public override string PortraitPath => $"res://mods/Komeiji_Koishi/images/cards/{GetType().Name}.png";

        protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
        {
            var player = base.Owner as MegaCrit.Sts2.Core.Entities.Players.Player;
            if (player == null) return;

            await CreatureCmd.TriggerAnim(player.Creature, "Buff", player.Character!.CastAnimDelay);
            
            await PowerCmd.Apply<PhilosophyOfTheHatedPower>(player.Creature, 1m, player.Creature, this, false);
        }

        protected override void OnUpgrade()
        {
            base.EnergyCost.UpgradeBy(-1); 
        }
    }
}