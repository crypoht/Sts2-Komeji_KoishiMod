using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BaseLib.Abstracts;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Models.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Nodes.CommonUi; 
using KomeijiKoishi.Pools;
using KomeijiKoishi.Enums;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Entities.Creatures;
using BaseLib.Utils; 
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using KomeijiKoishi.Powers; 

namespace KomeijiKoishi.Cards
{
    [Pool(typeof(KoishiCardPool))]
    public sealed class RecognizedGenius_Koishi : CustomCardModel
    {
        public RecognizedGenius_Koishi() 
            : base(2, CardType.Power, CardRarity.Uncommon, TargetType.Self, true) { }

        public override string PortraitPath => $"res://mods/Komeiji_Koishi/images/cards/{GetType().Name}.png";

        protected override IEnumerable<IHoverTip> ExtraHoverTips => new[] 
        { 
            HoverTipFactory.FromKeyword(KoishiKeywords.Danmaku),
        };

        protected override IEnumerable<DynamicVar> CanonicalVars => new[] 
        { 
            new PowerVar<KuugaPower>(4m) 
        };

        protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
        {
            try
            {
                var player = base.Owner as MegaCrit.Sts2.Core.Entities.Players.Player;
                if (player == null) return;

                await CreatureCmd.TriggerAnim(player.Creature, "Buff", player.Character!.CastAnimDelay);
                await PowerCmd.Apply<RecognizedGeniusPower>(player.Creature, 1m, player.Creature, this, false);
                await PowerCmd.Apply<KuugaPower>(player.Creature, base.DynamicVars["KuugaPower"].BaseValue, player.Creature, this, false);


                var piles = new[] { 
                    PileType.Hand.GetPile(player), 
                    PileType.Draw.GetPile(player), 
                    PileType.Discard.GetPile(player),
                    PileType.Exhaust.GetPile(player) 
                };

                List<CardModel> danmakuToUpgrade = new List<CardModel>();
                
                foreach (var pile in piles)
                {
                    if (pile != null)
                    {
                        var danmakuInPile = pile.Cards.Where(c => 
                            c.Tags != null && 
                            c.Tags.Contains(KoishiTags.Danmaku) && 
                            !c.IsUpgraded
                        );
                        danmakuToUpgrade.AddRange(danmakuInPile);
                    }
                }

                if (danmakuToUpgrade.Count > 0)
                {
                    foreach (CardModel cardModel in danmakuToUpgrade)
                    {
                        CardCmd.Upgrade(cardModel, CardPreviewStyle.None);
                        
                        await Cmd.Wait(0.05f, false);
                    }
                }
            }
            catch (Exception e)
            {
                MegaCrit.Sts2.Core.Logging.Log.Error($"[RecognizedGenius] 拦截报错: {e.Message}");
            }
        }

        protected override void OnUpgrade()
        {
            base.AddKeyword(CardKeyword.Retain);
            base.DynamicVars["KuugaPower"].UpgradeValueBy(1m);
        }
    }
}