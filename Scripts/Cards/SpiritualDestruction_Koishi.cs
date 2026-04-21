using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using BaseLib.Abstracts;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using KomeijiKoishi.Pools;
using KomeijiKoishi.Cards.Danmaku; 
using System;
using BaseLib.Utils;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Models.Cards;
using MegaCrit.Sts2.Core.ValueProps;
using KomeijiKoishi.Powers; 
using KomeijiKoishi.Enums;
using MegaCrit.Sts2.Core.Models.Powers; 
using MegaCrit.Sts2.Core.Nodes.CommonUi;
using KomeijiKoishi.Utils_Koishi;
using MegaCrit.Sts2.Core.HoverTips;

namespace KomeijiKoishi.Cards
{
    [Pool(typeof(KoishiCardPool))]
    public sealed class SpiritualDestruction_Koishi : CustomCardModel
    {
        public SpiritualDestruction_Koishi() 
            : base(-1, CardType.Skill, CardRarity.Rare, TargetType.Self, true)
        {
        }

        public override string PortraitPath => $"res://mods/Komeiji_Koishi/images/cards/{GetType().Name}.png";

        protected override bool HasEnergyCostX => true;

        protected override IEnumerable<IHoverTip> ExtraHoverTips => new[] 
        { 
            HoverTipFactory.FromCard<YinYangOrbDanmaku_Koishi>(base.IsUpgraded),
            HoverTipFactory.FromCard<ConsciousOverflow_Koishi>(false)
        };

        protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
        {
            var player = base.Owner as MegaCrit.Sts2.Core.Entities.Players.Player;
            if (player == null || base.CombatState == null) return;

            await CreatureCmd.TriggerAnim(player.Creature, "Cast", player.Character.CastAnimDelay);

            

            int xValue = base.ResolveEnergyXValue();
            if (xValue <= 0) return;


            for (int i = 0; i < xValue; i++)
            {
                var orbCard = base.CombatState.CreateCard<YinYangOrbDanmaku_Koishi>(player);
                
                if (base.IsUpgraded)
                {
                    CardCmd.Upgrade(orbCard, CardPreviewStyle.None);
                }

                KoishiExtensions.AutoPlayedByUnconsciousCards.Add(orbCard);

                await CardCmd.AutoPlay(choiceContext, orbCard, null, AutoPlayType.Default, false, false);

                KoishiExtensions.AutoPlayedByUnconsciousCards.Remove(orbCard);
            }

            int overflowCount = xValue * 2;
            var overflowCards = new List<CardModel>();
            
            for (int i = 0; i < overflowCount; i++)
            {
                overflowCards.Add(base.CombatState.CreateCard<ConsciousOverflow_Koishi>(player));
            }

            await CardPileCmd.AddGeneratedCardsToCombat(overflowCards, PileType.Draw, true, CardPilePosition.Random);
        }
    }
}