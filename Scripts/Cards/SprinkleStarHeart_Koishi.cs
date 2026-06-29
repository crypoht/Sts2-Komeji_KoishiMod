using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BaseLib.Abstracts;
using BaseLib.Utils;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.ValueProps;
using MegaCrit.Sts2.Core.HoverTips;
using KomeijiKoishi.Pools;
using KomeijiKoishi.Cards.Danmaku;
using KomeijiKoishi.Utils_Koishi;
using KomeijiKoishi.Enums;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Factories;



namespace KomeijiKoishi.Cards
{
    [Pool(typeof(KoishiCardPool))]
    public sealed class SprinkleStarHeart_Koishi : CustomCardModel
    {
        public SprinkleStarHeart_Koishi()
            : base(2, CardType.Attack, CardRarity.Uncommon, TargetType.AnyEnemy, true) 
        { 
        }

        public override string PortraitPath => KoishiImagePaths.CardPortrait(GetType());

        protected override IEnumerable<DynamicVar> CanonicalVars => new List<DynamicVar>
        {
            new DamageVar(4m, ValueProp.Move),
            new DynamicVar("HeartAmount", 1m)
        };

        protected override IEnumerable<IHoverTip> ExtraHoverTips => new List<IHoverTip>
        {
            HoverTipFactory.FromCard<StarDanmaku_Koishi>(false),
            HoverTipFactory.FromCard<HeartDanmaku_Koishi>(false),
            HoverTipFactory.FromKeyword(KoishiKeywords.Danmaku)
        };

        protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
        {
            var player = base.Owner as MegaCrit.Sts2.Core.Entities.Players.Player;
            if (player == null || cardPlay.Target == null || base.CombatState == null) return;


            await DamageCmd.Attack(base.DynamicVars.Damage.BaseValue)
                .FromCard(this)
                .Targeting(cardPlay.Target)
                .WithHitFx("vfx/vfx_attack_slash")
                .Execute(choiceContext);

            try
            {

                var starCard = base.CombatState.CreateCard<StarDanmaku_Koishi>(player);
                DanmakuPool.InheritEnchantment(this, starCard);

    
                if (starCard != null) 
                    await CardPileCmd.AddGeneratedCardToCombat(starCard, PileType.Exhaust, player, CardPilePosition.Bottom);

                for (int i = 0; i < base.DynamicVars["HeartAmount"].IntValue; i++)
                {
                    var heartCard = base.CombatState.CreateCard<HeartDanmaku_Koishi>(player);
                    DanmakuPool.InheritEnchantment(this, heartCard);
                    if (heartCard != null)
                        await CardPileCmd.AddGeneratedCardToCombat(heartCard, PileType.Exhaust, player, CardPilePosition.Bottom);
                }

      
                await Cmd.Wait(0.1f, false);


                List<CardModel> targetDanmakus = new List<CardModel>();
                PileType[] pilesToCheck = { PileType.Hand, PileType.Draw, PileType.Discard, PileType.Exhaust };

                foreach (var pType in pilesToCheck)
                {
                    var pile = pType.GetPile(player);
                    if (pile != null && pile.Cards != null)
                    {
                        var foundCards = pile.Cards.Where(c => c is StarDanmaku_Koishi || c is HeartDanmaku_Koishi).ToList();
                        targetDanmakus.AddRange(foundCards);
                    }
                }

                if (targetDanmakus.Count > 0)
                {
                    foreach (var danmaku in targetDanmakus)
                    {
                        var aliveEnemies = base.CombatState.HittableEnemies.Where(e => e != null && !e.IsDead).ToList();
                        if (aliveEnemies.Count == 0) break;
                        Creature? targetCreature = cardPlay.Target;

                        if (targetCreature == null || targetCreature.IsDead)
                        {
                            if (danmaku.TargetType == TargetType.AnyEnemy)
                            {
                                targetCreature = player.RunState.Rng.Shuffle.NextItem(aliveEnemies);
                            }
                        }

                        KoishiExtensions.AutoPlayedByUnconsciousCards.Add(danmaku);

                        await CardCmd.AutoPlay(choiceContext, danmaku, targetCreature, AutoPlayType.Default, false, false);
                        
                        KoishiExtensions.AutoPlayedByUnconsciousCards.Remove(danmaku);
                    }
                }
            }
            catch (Exception e)
            {
                MegaCrit.Sts2.Core.Logging.Log.Error($"[SprinkleStarHeart] 打出弹幕报错: {e.Message}");
            }
        }

        protected override void OnUpgrade()
        {
            base.DynamicVars["HeartAmount"].UpgradeValueBy(1m); 
        }
    }
}
