using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BaseLib.Abstracts;
using BaseLib.Utils;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Players; 
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.ValueProps;
using KomeijiKoishi.Pools;
using KomeijiKoishi.Enums;
using KomeijiKoishi.Cards.Danmaku; 

namespace KomeijiKoishi.Cards
{
    [Pool(typeof(KoishiCardPool))]
    public sealed class UnintentionalDanmaku_Koishi : CustomCardModel
    {
        public UnintentionalDanmaku_Koishi() 
            : base(1, CardType.Skill, CardRarity.Common, TargetType.Self, true) { }

        public override string PortraitPath => $"res://mods/Komeiji_Koishi/images/cards/{GetType().Name}.png";

        protected override IEnumerable<IHoverTip> ExtraHoverTips => new[] 
        { 
            HoverTipFactory.FromKeyword(KoishiKeywords.Danmaku) 
        };

        protected override IEnumerable<DynamicVar> CanonicalVars => new List<DynamicVar> 
        { 
            new BlockVar(6m, ValueProp.Move), 
            new DynamicVar("Amount", 1m)     
        };

        protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
        {
            try
            {
                var player = base.Owner as Player;
                if (player == null) return;

                await CreatureCmd.GainBlock(player.Creature, base.DynamicVars.Block.BaseValue, ValueProp.Move, cardPlay, false);

                if (base.CombatState != null)
                {
                    int count = (int)base.DynamicVars["Amount"].BaseValue;
                    List<CardModel> generatedCards = new List<CardModel>();

                    for (int i = 0; i < count; i++)
                    {
                        var randomCardModel = player.RunState.Rng.Shuffle.NextItem(DanmakuPool.Pool);
                        
                        if (randomCardModel != null)
                        {
                            var newCard = base.CombatState.CreateCard(randomCardModel, player);
                            generatedCards.Add(newCard);
                        }
                    }

                    if (generatedCards.Count > 0)
                    {
                        await CardPileCmd.AddGeneratedCardsToCombat(generatedCards, PileType.Hand, true, CardPilePosition.Bottom);
                    }
                }
            }
            catch (Exception e)
            {
                MegaCrit.Sts2.Core.Logging.Log.Error($"[UnintentionalDanmaku] Error: {e.Message}");
            }
        }

        protected override void OnUpgrade()
        {
            base.DynamicVars.Block.UpgradeValueBy(1m);
            base.DynamicVars["Amount"].UpgradeValueBy(1m);
        }
    }
}