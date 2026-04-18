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
    public sealed class SecondNeed_Koishi : CustomCardModel
    {
        public SecondNeed_Koishi() 
            : base(1, CardType.Power, CardRarity.Uncommon, TargetType.Self, true) { }

        public override string PortraitPath => $"res://mods/Komeiji_Koishi/images/cards/{GetType().Name}.png";

        protected override IEnumerable<DynamicVar> CanonicalVars => new List<DynamicVar> 
        { 
            new BlockVar(3m, ValueProp.Move), 
        };

        protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
        {
            var player = base.Owner as MegaCrit.Sts2.Core.Entities.Players.Player;
            if (player == null) return;

            await CreatureCmd.TriggerAnim(player.Creature, "Buff", player.Character!.CastAnimDelay);

            decimal amount = base.DynamicVars.Block.BaseValue;
            
            await PowerCmd.Apply<SecondNeedPower>(player.Creature, amount, player.Creature, this, false);
        }

        protected override void OnUpgrade()
        {
            base.DynamicVars.Block.UpgradeValueBy(2m);
        }
    }
}