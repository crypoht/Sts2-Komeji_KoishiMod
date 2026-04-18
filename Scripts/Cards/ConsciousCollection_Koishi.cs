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
using MegaCrit.Sts2.Core.HoverTips;
using KomeijiKoishi.Pools;
using KomeijiKoishi.Cards.Danmaku;
using KomeijiKoishi.Powers; 
using KomeijiKoishi.Utils_Koishi;
using System;
using BaseLib.Utils;     
using KomeijiKoishi.Enums;
 

namespace KomeijiKoishi.Cards
{
    [Pool(typeof(KoishiCardPool))]
    public sealed class ConsciousCollection_Koishi : CustomCardModel
    {
        public ConsciousCollection_Koishi() 
            : base(0, CardType.Skill, CardRarity.Common, TargetType.Self, true) { }

        public override string PortraitPath => $"res://mods/Komeiji_Koishi/images/cards/{GetType().Name}.png";
        protected override IEnumerable<IHoverTip> ExtraHoverTips => new[] 
        { 
            HoverTipFactory.FromCard<ConsciousOverflow_Koishi>(false)
        };

        protected override IEnumerable<DynamicVar> CanonicalVars => new List<DynamicVar> 
        { 
            new CardsVar(2) 
        };

        protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
        {
            var player = base.Owner as MegaCrit.Sts2.Core.Entities.Players.Player;
            if (player == null || base.CombatState == null) return;

            await CreatureCmd.TriggerAnim(player.Creature, "Cast", player.Character!.CastAnimDelay);

            await CardPileCmd.Draw(choiceContext, base.DynamicVars.Cards.BaseValue, player, false);

            var token = base.CombatState.CreateCard<ConsciousOverflow_Koishi>(player);
            if (token != null)
            {
                await CardPileCmd.AddGeneratedCardToCombat(token, PileType.Hand, true, CardPilePosition.Bottom);
            }
        }

        protected override void OnUpgrade()
        {
            base.DynamicVars.Cards.UpgradeValueBy(1m);
        }
    }
}