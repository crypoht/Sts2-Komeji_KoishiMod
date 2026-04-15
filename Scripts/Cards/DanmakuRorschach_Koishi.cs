using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BaseLib.Utils;
using BaseLib.Abstracts;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using KomeijiKoishi.Pools;
using KomeijiKoishi.Utils_Koishi;
using KomeijiKoishi.Enums;
using MegaCrit.Sts2.Core.HoverTips;

namespace KomeijiKoishi.Cards
{
    [Pool(typeof(KoishiCardPool))]
    public sealed class DanmakuRorschach_Koishi : CustomCardModel
    {
        public DanmakuRorschach_Koishi()
            : base(3, CardType.Skill, CardRarity.Rare, TargetType.AnyEnemy, true) { }

        public override string PortraitPath => $"res://mods/Komeiji_Koishi/images/cards/{GetType().Name}.png";

        protected override IEnumerable<IHoverTip> ExtraHoverTips => new[] 
        { 
            HoverTipFactory.FromKeyword(KoishiKeywords.Danmaku) 
        };

        protected override HashSet<CardTag> CanonicalTags => new HashSet<CardTag> { KoishiTags.Subconscious };

        public override IEnumerable<CardKeyword> CanonicalKeywords
        {
            get
            {
                try { return KoishiExtensions.IsTrulyUnconscious(this) ? new[] { KoishiKeywords.Unconscious } : new CardKeyword[0]; }
                catch (Exception) { return new CardKeyword[0]; }
            }
        }

        protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
        {
            try
            {
                var player = base.Owner as MegaCrit.Sts2.Core.Entities.Players.Player;
                if (player == null) return;

                await CreatureCmd.TriggerAnim(player.Creature, "Cast", player.Character!.CastAnimDelay);

                List<CardModel> allDanmakusSnapshot = new List<CardModel>();
                PileType[] pilesToCheck = { PileType.Hand, PileType.Draw, PileType.Discard, PileType.Exhaust };

                foreach (var pType in pilesToCheck)
                {
                    var pile = pType.GetPile(player);
                    if (pile != null && pile.Cards != null)
                    {
                        var danmakusInPile = pile.Cards.Where(c => c.Tags != null && c.Tags.Contains(KoishiTags.Danmaku)).ToList();
                        allDanmakusSnapshot.AddRange(danmakusInPile);
                    }
                }

                if (allDanmakusSnapshot.Count > 0)
                {
                    foreach (var danmaku in allDanmakusSnapshot)
                    {
                        var aliveEnemies = base.CombatState!.HittableEnemies.Where(e => e != null && !e.IsDead).ToList();
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
                        
                        await Cmd.Wait(0.15f, false); 
                    }
                }
            }
            catch (Exception e)
            {
                MegaCrit.Sts2.Core.Logging.Log.Error($"[DanmakuRorschach] 拦截报错: {e.Message}");
            }
        }

        protected override void OnUpgrade()
        {
            base.EnergyCost.UpgradeBy(-1); 
        }
    }
}