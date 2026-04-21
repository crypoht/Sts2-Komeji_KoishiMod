using System.Threading.Tasks;
using BaseLib.Abstracts;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Models.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using KomeijiKoishi.Pools;
using KomeijiKoishi.Powers;
using System;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.CardPools;
using System.Linq;
using BaseLib.Utils;
using MegaCrit.Sts2.Core.HoverTips; 
using MegaCrit.Sts2.Core.ValueProps;
using KomeijiKoishi.Cards.Danmaku;

namespace KomeijiKoishi.Cards
{
    [Pool(typeof(KoishiCardPool))]
    public sealed class SelflessLove_Koishi : CustomCardModel
    {
        public SelflessLove_Koishi() 
            : base(1, CardType.Power, CardRarity.Uncommon, TargetType.Self, true) { }

        public override string PortraitPath => $"res://mods/Komeiji_Koishi/images/cards/{GetType().Name}.png";

        protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
        {
            var player = base.Owner as MegaCrit.Sts2.Core.Entities.Players.Player;
            if (player == null) return;

            // 播放施法动画
            await CreatureCmd.TriggerAnim(player.Creature, "Buff", player.Character!.CastAnimDelay);
            
            // 给自己施加 1 层“没我之爱”能力（代表每次触发抽 1 张牌）
            await PowerCmd.Apply<SelflessLovePower>(player.Creature, 1m, player.Creature, this, false);
        }

        protected override void OnUpgrade()
        {
            // 🌟 升级效果：费用从 1 降为 0
            base.EnergyCost.UpgradeBy(-1);
        }
    }
}