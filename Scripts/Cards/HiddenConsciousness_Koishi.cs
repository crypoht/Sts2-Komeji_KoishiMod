using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BaseLib.Abstracts;
using BaseLib.Utils;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.ValueProps;
using KomeijiKoishi.Pools;
using KomeijiKoishi.Enums;
using KomeijiKoishi.Utils_Koishi;
using KomeijiKoishi.Cards.Danmaku; 

namespace KomeijiKoishi.Cards
{
    [Pool(typeof(KoishiCardPool))]
    public sealed class HiddenConsciousness_Koishi : CustomCardModel
    {
        public HiddenConsciousness_Koishi() 
            : base(2, CardType.Skill, CardRarity.Uncommon, TargetType.Self, true) { }

        public override string PortraitPath => $"res://mods/Komeiji_Koishi/images/cards/{GetType().Name}.png";

        protected override IEnumerable<DynamicVar> CanonicalVars => new List<DynamicVar> 
        { 
            new BlockVar(14m, ValueProp.Move),
            new CardsVar(2)
        };

        protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
        {
            try
            {
                var player = base.Owner as Player;
                if (player == null || base.CombatState == null) return;

                await CreatureCmd.GainBlock(player.Creature, base.DynamicVars.Block.BaseValue, ValueProp.Move, cardPlay, false);

                for (int i = 0; i < 3; i++)
                {
                    await DanmakuPool.CreateRandomDanmakuInExhaust(player, base.CombatState);
                }

                await Cmd.Wait(0.1f, false);

                var availableDanmaku = PileType.Exhaust.GetPile(player)
                    .Cards
                    .Where(c => c.Tags != null && c.Tags.Contains(KoishiTags.Danmaku))
                    .ToList();

                int playCount = base.DynamicVars.Cards.IntValue;
                
                List<CardModel> danmakuToPlay = new List<CardModel>();
                var tempAvailable = new List<CardModel>(availableDanmaku);
                
                for (int i = 0; i < playCount; i++)
                {
                    if (tempAvailable.Count <= 0) break;
                    var card = player.RunState.Rng.Shuffle.NextItem(tempAvailable);

                    if (card != null)
                    {
                        danmakuToPlay.Add(card);
                        tempAvailable.Remove(card); 
                    }
                }

                foreach (CardModel cardModel in danmakuToPlay)
                {
                    Creature? targetCreature = null;

                    if (cardModel.TargetType == TargetType.AnyEnemy)
                    {
                        var combatState = player.Creature.CombatState; 
                        if (combatState != null)
                        {
                            var validEnemies = combatState.HittableEnemies.Where(e => !e.IsDead).ToList();
                            if (validEnemies.Count > 0)
                            {
                                targetCreature = player.RunState.Rng.Shuffle.NextItem(validEnemies);
                            }
                        }
                    }
                    KoishiExtensions.AutoPlayedByUnconsciousCards.Add(cardModel);

                    await CardCmd.AutoPlay(choiceContext, cardModel, targetCreature, AutoPlayType.Default, true, false);

                    KoishiExtensions.AutoPlayedByUnconsciousCards.Remove(cardModel);
                    
                    await Cmd.Wait(0.1f, false);
                }
            }
            catch (Exception e)
            {
                MegaCrit.Sts2.Core.Logging.Log.Error($"[HiddenConsciousness_Koishi] Error: {e.Message}");
            }
        }

        protected override void OnUpgrade()
        {
            base.DynamicVars.Block.UpgradeValueBy(4m);
        }
    }
}