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
            HoverTipFactory.FromKeyword(KoishiKeywords.Danmaku) 
        };

        public override IEnumerable<CardKeyword> CanonicalKeywords
        {
            get
            {
                try { return KoishiExtensions.IsUnconscious(this) ? new[] { KoishiKeywords.Unconscious } : new CardKeyword[0]; }
                catch (Exception) { return new CardKeyword[0]; }
            }
        }

        protected override IEnumerable<DynamicVar> CanonicalVars => new List<DynamicVar> { new CardsVar(3) };

        protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
        {
            try
            {
                await CreatureCmd.TriggerAnim(base.Owner!.Creature, "Cast", base.Owner.Character!.CastAnimDelay);

                IEnumerable<CardModel> generatedCards = CardFactory.GetForCombat(
                    base.Owner, 
                    DanmakuPool.Pool, 
                    base.DynamicVars.Cards.IntValue, 
                    base.Owner.RunState!.Rng!.CombatCardGeneration!
                );

                Random rng = new Random(); 

                foreach (CardModel generatedDanmaku in generatedCards)
                {
                    var aliveEnemies = base.CombatState!.HittableEnemies.Where(e => e != null && !e.IsDead).ToList();
                    if (aliveEnemies.Count == 0) break;

                    Creature? targetCreature = null;
                    if (generatedDanmaku.TargetType == TargetType.AnyEnemy)
                    {
                        targetCreature = aliveEnemies[rng.Next(aliveEnemies.Count)]!;
                    }

                    await CardCmd.AutoPlay(choiceContext, generatedDanmaku, targetCreature, AutoPlayType.Default, true, false);
                    await Cmd.Wait(0.15f, false);
                }
            }
            catch (Exception e)
            {
                Console.WriteLine($"[UnconsciousDanmakuAttack] FATAL ERROR PREVENTED: {e}");
            }
        }

        protected override void OnUpgrade()
        {
            base.DynamicVars.Cards.UpgradeValueBy(1m);
        }
    }
}