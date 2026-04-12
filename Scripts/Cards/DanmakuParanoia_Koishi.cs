using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BaseLib.Utils;     
using BaseLib.Abstracts; 
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Cards;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer; 
using MegaCrit.Sts2.Core.Factories; 
using KomeijiKoishi.Pools; 
using KomeijiKoishi.Utils_Koishi; 
using KomeijiKoishi.Enums;
using KomeijiKoishi.Cards.Danmaku; 


namespace KomeijiKoishi.Cards
{
    [Pool(typeof(KoishiCardPool))]
    public sealed class DanmakuParanoia_Koishi : CustomCardModel
    {
        public DanmakuParanoia_Koishi() 
            : base(1, CardType.Skill, CardRarity.Rare, TargetType.Self, true) { }

        public override string PortraitPath => $"res://mods/Komeiji_Koishi/images/cards/{GetType().Name}.png";
        public override IEnumerable<CardKeyword> CanonicalKeywords => new[] { CardKeyword.Exhaust };

        protected override IEnumerable<IHoverTip> ExtraHoverTips => new[] 
        { 
            HoverTipFactory.FromKeyword(KoishiKeywords.Unconscious),
            HoverTipFactory.FromKeyword(KoishiKeywords.Danmaku) 
        };
        protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
        {
            try 
            {
                var player = base.Owner as MegaCrit.Sts2.Core.Entities.Players.Player;
                if (player == null) return;

                await CreatureCmd.TriggerAnim(player.Creature, "Cast", player.Character!.CastAnimDelay);

                List<CardModel> handCards = PileType.Hand.GetPile(base.Owner).Cards.ToList();
                int exhaustedCount = 0;

                foreach (CardModel card in handCards)
                {
                    bool isUnconscious = false;
                    
                    try 
                    {
                        isUnconscious = KoishiExtensions.IsUnconscious(card);
                    }
                    catch 
                    {
                        isUnconscious = (card.Tags != null && card.Tags.Contains(KoishiTags.Unconscious)) ||
                                        (card.Keywords != null && card.Keywords.Contains(KoishiKeywords.Unconscious));
                    }

                    if (!isUnconscious && card != this)
                    {
                        await CardCmd.Exhaust(choiceContext, card, false, false);
                        exhaustedCount++;
                    }
                }

                if (exhaustedCount > 0)
                {
                    if (DanmakuPool.Pool == null || DanmakuPool.Pool.Count == 0)
                    {
                        MegaCrit.Sts2.Core.Logging.Log.Error("[DanmakuParanoia] 弹幕池为空！请检查注册逻辑。");
                        return; 
                    }

                    IEnumerable<CardModel> generatedCards = CardFactory.GetForCombat(
                        base.Owner, 
                        DanmakuPool.Pool, 
                        exhaustedCount, 
                        player.RunState!.Rng.CombatCardGeneration
                    );

                    foreach (CardModel generatedDanmaku in generatedCards)
                    {
                        await CardPileCmd.AddGeneratedCardToCombat(generatedDanmaku, PileType.Hand, false, CardPilePosition.Random);
                    }
                }
            }
            catch (Exception ex)
            {
                MegaCrit.Sts2.Core.Logging.Log.Error($"[DanmakuParanoia] 拦截到 Bug: {ex.Message}");
            }
        }

        protected override void OnUpgrade()
        {
            base.AddKeyword(CardKeyword.Retain);
        }
    }
}