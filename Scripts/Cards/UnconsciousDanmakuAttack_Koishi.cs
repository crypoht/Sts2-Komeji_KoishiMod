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
using MegaCrit.Sts2.Core.Factories; 
using MegaCrit.Sts2.Core.GameActions.Multiplayer; 
using KomeijiKoishi.Pools; 
using KomeijiKoishi.Enums;
using KomeijiKoishi.Utils_Koishi; 
using KomeijiKoishi.Cards.Danmaku;
using MegaCrit.Sts2.Core.HoverTips; 
using KomeijiKoishi.Powers;

namespace KomeijiKoishi.Cards
{
    [Pool(typeof(KoishiCardPool))]
    public sealed class UnconsciousDanmakuAttack_Koishi : CustomCardModel
    {
        public UnconsciousDanmakuAttack_Koishi() 
            : base(2, CardType.Skill, CardRarity.Uncommon, TargetType.Self, true) { }

        public override string PortraitPath => $"res://mods/Komeiji_Koishi/images/cards/{GetType().Name}.png";
        
        protected override HashSet<CardTag> CanonicalTags => new HashSet<CardTag> { KoishiTags.Subconscious };

        protected override IEnumerable<IHoverTip> ExtraHoverTips => new[] 
        { 
            HoverTipFactory.FromKeyword(KoishiKeywords.Danmaku),
            HoverTipFactory.FromKeyword(KoishiKeywords.Unconscious) 
        };

        public override IEnumerable<CardKeyword> CanonicalKeywords => new CardKeyword[0];

        protected override IEnumerable<DynamicVar> CanonicalVars => new List<DynamicVar> { new CardsVar(4) };

        protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
        {
            var player = base.Owner as MegaCrit.Sts2.Core.Entities.Players.Player;
            if (player == null || base.CombatState == null) return;

            try
            {
                await CreatureCmd.TriggerAnim(player.Creature, "Cast", player.Character!.CastAnimDelay);

                int danmakuCount = base.DynamicVars.Cards.IntValue;

                for (int i = 0; i < danmakuCount; i++)
                {
                    var generatedDanmaku = await DanmakuPool.CreateRandomDanmakuInHand(player, base.CombatState);
                    
                    if (generatedDanmaku != null)
                    {
                        var aliveEnemies = base.CombatState.HittableEnemies.Where(e => e != null && !e.IsDead).ToList();
                        if (aliveEnemies.Count == 0) break;

                        Creature? targetCreature = null;
                        if (generatedDanmaku.TargetType == TargetType.AnyEnemy)
                        {
                            targetCreature = player.RunState.Rng.Shuffle.NextItem(aliveEnemies);
                        }

                        KoishiExtensions.AutoPlayedByUnconsciousCards.Add(generatedDanmaku);
                        
                        await CardCmd.AutoPlay(choiceContext, generatedDanmaku, targetCreature, AutoPlayType.Default, true, false);
                        
                        KoishiExtensions.AutoPlayedByUnconsciousCards.Remove(generatedDanmaku);
                    }
                }
            }
            catch (Exception e)
            {
                MegaCrit.Sts2.Core.Logging.Log.Error($"[UnconsciousDanmakuAttack] ERROR: {e}");
            }
            finally
            {
            }
        }

        protected override void OnUpgrade()
        {
            base.DynamicVars.Cards.UpgradeValueBy(1m);
        }
    }
}