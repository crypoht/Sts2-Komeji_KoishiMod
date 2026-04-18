using System.Collections.Generic;
using System.Threading.Tasks;
using BaseLib.Abstracts;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Models.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using KomeijiKoishi.Pools;
using KomeijiKoishi.Powers;
using MegaCrit.Sts2.Core.HoverTips;
using KomeijiKoishi.Enums;
using System;
using System.Linq;
using BaseLib.Utils;     
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.Factories;  
using KomeijiKoishi.Utils_Koishi; 
using KomeijiKoishi.Cards.Danmaku; 


namespace KomeijiKoishi.Cards
{
    [Pool(typeof(KoishiCardPool))]
    public sealed class DanmakuEndlessMiracle_Koishi : CustomCardModel
    {
        public DanmakuEndlessMiracle_Koishi() 
            : base(1, CardType.Skill, CardRarity.Rare, TargetType.Self, true) { }

        public override string PortraitPath => $"res://mods/Komeiji_Koishi/images/cards/{GetType().Name}.png";

        protected override IEnumerable<IHoverTip> ExtraHoverTips => new[] 
        { 
            HoverTipFactory.FromKeyword(KoishiKeywords.Danmaku) 
        };

        protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
        {
            var player = base.Owner as MegaCrit.Sts2.Core.Entities.Players.Player;
            if (player == null) return;

            await CreatureCmd.TriggerAnim(player.Creature, "Buff", player.Character!.CastAnimDelay);

            await PowerCmd.Apply<MiraclePower>(player.Creature, 1m, player.Creature, this, false);
        }

        protected override void OnUpgrade()
        {

            base.EnergyCost.UpgradeBy(-1);
        }
    }
}