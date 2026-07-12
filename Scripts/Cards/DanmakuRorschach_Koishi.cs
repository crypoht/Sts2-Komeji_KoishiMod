using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BaseLib.Utils;
using BaseLib.Abstracts;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
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

        public override string PortraitPath => KoishiImagePaths.CardPortrait(GetType());

        protected override IEnumerable<IHoverTip> ExtraHoverTips => new[] 
        { 
            HoverTipFactory.FromKeyword(KoishiKeywords.Danmaku),
        };

        protected override HashSet<CardTag> CanonicalTags => new HashSet<CardTag> { KoishiTags.Subconscious };
        public override IEnumerable<CardKeyword> CanonicalKeywords => new CardKeyword[0];

        protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
        {
            try
            {
                var player = base.Owner as Player;
                if (player == null) return;

                await CreatureCmd.TriggerAnim(player.Creature, "Cast", player.Character!.CastAnimDelay);

                List<CardModel> allDanmakusSnapshot = new List<CardModel>();
                PileType[] pilesToCheck = { PileType.Hand, PileType.Draw, PileType.Discard, PileType.Exhaust };

                var playersToCheck = base.CombatState?.Players
                    .Where(p => p?.Creature != null && p.Creature.Side == player.Creature.Side)
                    .ToList() ?? new List<Player> { player };

                foreach (Player pileOwner in playersToCheck)
                {
                    foreach (var pType in pilesToCheck)
                    {
                        var pile = pType.GetPile(pileOwner);
                        if (pile != null && pile.Cards != null)
                        {
                            var danmakusInPile = pile.Cards
                                .Where(c => c.Tags != null && c.Tags.Contains(KoishiTags.Danmaku))
                                .ToList();
                            allDanmakusSnapshot.AddRange(danmakusInPile);
                        }
                    }
                }

                if (allDanmakusSnapshot.Count > 0)
                {
                    foreach (var danmaku in allDanmakusSnapshot)
                    {
                        Player danmakuOwner = danmaku.Owner ?? player;
                        var aliveEnemies = danmakuOwner.Creature.CombatState!.HittableEnemies.Where(e => e != null && !e.IsDead).ToList();
                        if (aliveEnemies.Count == 0) break; 

                        Creature? targetCreature = GetAutoTarget(danmakuOwner, danmaku, cardPlay.Target);
                        await KoishiExtensions.SafeAutoPlayCard(choiceContext, danmakuOwner, danmaku, targetCreature, AutoPlayType.Default, false, false);
                        
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

        private Creature? GetAutoTarget(Player player, CardModel card, Creature? originalTarget)
        {
            if (player.Creature.CombatState == null)
            {
                return null;
            }

            if (card.TargetType == TargetType.AnyEnemy)
            {
                if (originalTarget != null && !originalTarget.IsDead && originalTarget.Side != player.Creature.Side)
                {
                    return originalTarget;
                }

                var enemies = player.Creature.CombatState.HittableEnemies.Where(e => e != null && !e.IsDead).ToList();
                return enemies.Count > 0 ? player.RunState.Rng.Shuffle.NextItem(enemies) : null;
            }

            if (card.TargetType == TargetType.AnyAlly)
            {
                var allies = player.Creature.CombatState.Players
                    .Where(p => p != null && p != player && p.Creature != null && !p.Creature.IsDead && p.Creature.Side == player.Creature.Side)
                    .Select(p => p.Creature)
                    .ToList();

                return allies.Count > 0 ? player.RunState.Rng.Shuffle.NextItem(allies) : null;
            }

            if (card.TargetType == TargetType.AnyPlayer)
            {
                return player.Creature;
            }

            return null;
        }
    }
}
