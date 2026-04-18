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
using KomeijiKoishi.Utils_Koishi; 

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

        protected override IEnumerable<DynamicVar> CanonicalVars => new List<DynamicVar> { new CardsVar(7) };

        protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
        {
            try
            {
                var player = base.Owner as MegaCrit.Sts2.Core.Entities.Players.Player;
                if (player == null) return;

                await CreatureCmd.TriggerAnim(player.Creature, "Cast", player.Character!.CastAnimDelay);

                List<CardModel> availableDanmaku = PileType.Exhaust.GetPile(player)
                    .Cards
                    .Where(c => c.Tags != null && c.Tags.Contains(KoishiTags.Danmaku)) 
                    .ToList();

                int playCount = base.DynamicVars.Cards.IntValue;
                List<CardModel> danmakuToPlay = new List<CardModel>();

                for (int i = 0; i < playCount; i++)
                {
                    if (availableDanmaku.Count == 0) break;
                    
                    var card = player.RunState.Rng.Shuffle.NextItem(availableDanmaku);
                    if (card != null)
                    {
                        danmakuToPlay.Add(card);
                        availableDanmaku.Remove(card); 
                    }
                }

                foreach (CardModel cardModel in danmakuToPlay)
                {
                    Creature? targetCreature = null;

                    if (cardModel.TargetType == TargetType.AnyEnemy)
                    {
                        targetCreature = cardPlay.Target;
                    }

                    KoishiExtensions.AutoPlayedByUnconsciousCards.Add(cardModel);

                    await CardCmd.AutoPlay(choiceContext, cardModel, targetCreature, AutoPlayType.Default, true, false);
                    
                    KoishiExtensions.AutoPlayedByUnconsciousCards.Remove(cardModel);

                    await Cmd.Wait(0.15f, false);
                }
            }
            catch (Exception e)
            {
                MegaCrit.Sts2.Core.Logging.Log.Error($"[SubconsciousRelease] FATAL ERROR PREVENTED: {e}");
            }
        }

        protected override void OnUpgrade()
        {
            base.DynamicVars.Cards.UpgradeValueBy(1m);
        }
    }
}