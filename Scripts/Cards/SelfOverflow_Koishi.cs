using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using BaseLib.Abstracts;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using KomeijiKoishi.Pools;
using MegaCrit.Sts2.Core.Models.CardPools;
using System.Linq;
using BaseLib.Utils;
using MegaCrit.Sts2.Core.HoverTips; 
using MegaCrit.Sts2.Core.ValueProps;
using KomeijiKoishi.Cards.Danmaku;

namespace KomeijiKoishi.Cards
{
    [Pool(typeof(KoishiCardPool))]
    public sealed class SelfOverflow_Koishi : CustomCardModel
    {
        public SelfOverflow_Koishi() 
            : base(0, CardType.Skill, CardRarity.Uncommon, TargetType.Self, true) { }

        public override string PortraitPath => $"res://mods/Komeiji_Koishi/images/cards/{GetType().Name}.png";

        protected override IEnumerable<DynamicVar> CanonicalVars => new List<DynamicVar> 
        { 
            new EnergyVar(2),
            new CardsVar(2)  
        };

        protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
        {
            var player = base.Owner as MegaCrit.Sts2.Core.Entities.Players.Player;
            if (player == null || base.CombatState == null) return;


            await CreatureCmd.TriggerAnim(player.Creature, "Buff", player.Character!.CastAnimDelay);

  
            int energyGain = base.DynamicVars.Energy.IntValue;
            await PlayerCmd.GainEnergy(energyGain, player);


            for (int i = 0; i < 2; i++)
            {
                var token = base.CombatState.CreateCard<ConsciousOverflow_Koishi>(player);
                if (token != null)
                {
                    await CardPileCmd.AddGeneratedCardToCombat(token, PileType.Hand, true, CardPilePosition.Bottom);
                }
            }
        }

        protected override void OnUpgrade()
        {

            base.DynamicVars.Energy.UpgradeValueBy(1m);
        }
    }
}