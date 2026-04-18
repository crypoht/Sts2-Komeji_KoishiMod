using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using BaseLib.Abstracts;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Models.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using KomeijiKoishi.Pools;
using KomeijiKoishi.Powers;
using System.Linq;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Nodes.CommonUi; 
using KomeijiKoishi.Enums;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Entities.Creatures;
using BaseLib.Utils; 
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models; 
using KomeijiKoishi.Cards.Danmaku;

namespace KomeijiKoishi.Cards
{
    [Pool(typeof(KoishiCardPool))]
    public sealed class ReimuHelp_Koishi : CustomCardModel
    {
        public ReimuHelp_Koishi() 
            : base(1, CardType.Power, CardRarity.Rare, TargetType.Self, true) { }

        public override string PortraitPath => $"res://mods/Komeiji_Koishi/images/cards/{GetType().Name}.png";

        protected override IEnumerable<IHoverTip> ExtraHoverTips => new[] 
        { 
            HoverTipFactory.FromCard<YinYangOrbDanmaku_Koishi>(false)
        };

        protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
        {
            var player = base.Owner as MegaCrit.Sts2.Core.Entities.Players.Player;
            if (player == null) return;

            await CreatureCmd.TriggerAnim(player.Creature, "Buff", player.Character!.CastAnimDelay);

            await PowerCmd.Apply<ReimuHelpPower>(player.Creature, 1m, player.Creature, this, false);
        }

        protected override void OnUpgrade()
        {
            base.AddKeyword(CardKeyword.Retain);
        }
    }
}