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
using KomeijiKoishi.Enums;

namespace KomeijiKoishi.Cards
{
    [Pool(typeof(KoishiCardPool))]
    public sealed class SubconsciousRelease_Koishi : CustomCardModel
    {
        public SubconsciousRelease_Koishi() 
            : base(3, CardType.Skill, CardRarity.Uncommon, TargetType.AnyEnemy, true) { }

        public override string PortraitPath => $"res://mods/Komeiji_Koishi/images/cards/{GetType().Name}.png";
        protected override HashSet<CardTag> CanonicalTags => new HashSet<CardTag> { KoishiTags.Unconscious };

        public override IEnumerable<CardKeyword> CanonicalKeywords => new[] { KoishiKeywords.Unconscious };

        protected override IEnumerable<DynamicVar> CanonicalVars => new List<DynamicVar> { new CardsVar(3) };

        protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
        {
            try
            {
                await CreatureCmd.TriggerAnim(base.Owner!.Creature, "Cast", base.Owner.Character!.CastAnimDelay);

                Random rng = new Random();
                List<CardModel> availableDanmaku = PileType.Exhaust.GetPile(base.Owner)
                    .Cards
                    .Where(c => c.Tags.Contains(KoishiTags.Danmaku)) 
                    .OrderBy(x => rng.Next()) 
                    .ToList();

                int playCount = base.DynamicVars.Cards.IntValue;
                List<CardModel> danmakuToPlay = availableDanmaku.Take(playCount).ToList();

                foreach (CardModel cardModel in danmakuToPlay)
                {
                    Creature? targetCreature = null;

                    if (cardModel.TargetType == TargetType.AnyEnemy)
                    {
                        targetCreature = cardPlay.Target;
                    }

                    await CardCmd.AutoPlay(choiceContext, cardModel, targetCreature, AutoPlayType.Default, true, false);
                    await Cmd.Wait(0.15f, false);
                }
            }
            catch (Exception e)
            {
                Console.WriteLine($"[SubconsciousRelease] FATAL ERROR PREVENTED: {e}");
            }
        }

        protected override void OnUpgrade()
        {
            base.DynamicVars.Cards.UpgradeValueBy(1m);
        }
    }
}