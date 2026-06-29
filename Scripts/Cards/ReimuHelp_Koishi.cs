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
            : base(3, CardType.Power, CardRarity.Rare, TargetType.Self, true) { }

        public override string PortraitPath => KoishiImagePaths.CardPortrait(GetType());

        protected override IEnumerable<IHoverTip> ExtraHoverTips => new[] 
        { 
            HoverTipFactory.FromCard<YinYangOrbDanmaku_Koishi>(false)
        };

        private class OrbsVar : DynamicVar { public OrbsVar(decimal val) : base("Orbs", val) { } }

        protected override IEnumerable<DynamicVar> CanonicalVars => new List<DynamicVar> 
        { 
            new OrbsVar(3m) 
        };

        protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
        {
            var player = base.Owner as MegaCrit.Sts2.Core.Entities.Players.Player;
            if (player == null) return;


            await CreatureCmd.TriggerAnim(player.Creature, "Buff", player.Character!.CastAnimDelay);

            if (base.CombatState != null)
            {
                var drawTokens = new List<CardModel>();

                int count = (int)base.DynamicVars["Orbs"].BaseValue;

                for (int i = 0; i < count; i++)
                {
                    CardModel token = base.CombatState.CreateCard<YinYangOrbDanmaku_Koishi>(player);
                    DanmakuPool.InheritEnchantment(this, token);
                    drawTokens.Add(token);
                }
                
                await CardPileCmd.AddGeneratedCardsToCombat(drawTokens, PileType.Draw, player, CardPilePosition.Random);
            }

            await PowerCmd.Apply<ReimuHelpPower>(choiceContext,player.Creature, 20m, player.Creature, this, false);
        }

        protected override void OnUpgrade()
        {
            base.AddKeyword(CardKeyword.Retain);
        }
    }
}
