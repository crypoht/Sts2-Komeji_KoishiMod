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
using KomeijiKoishi.Utils_Koishi;
using KomeijiKoishi.Enums;

namespace KomeijiKoishi.Cards
{
    [Pool(typeof(KoishiCardPool))]
    public sealed class CasualGame_Koishi : CustomCardModel
    {
        public CasualGame_Koishi() 
            : base(1, CardType.Skill, CardRarity.Uncommon, TargetType.Self, true) { }

        public override string PortraitPath => $"res://mods/Komeiji_Koishi/images/cards/{GetType().Name}.png";

        protected override IEnumerable<IHoverTip> ExtraHoverTips => new[] 
        { 
            HoverTipFactory.FromKeyword(KoishiKeywords.Unconscious) 
        };

        protected override IEnumerable<DynamicVar> CanonicalVars => new List<DynamicVar> 
        { 
            new CardsVar(2) 
        };

        protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
        {
            try
            {
                var player = base.Owner as MegaCrit.Sts2.Core.Entities.Players.Player;
                if (player == null || base.CombatState == null) return;

                await CreatureCmd.TriggerAnim(player.Creature, "Cast", player.Character!.CastAnimDelay);

                await CardPileCmd.Draw(choiceContext, base.DynamicVars.Cards.BaseValue, player, false);

                await Cmd.Wait(0.1f, false);

                var handPile = PileType.Hand.GetPile(player);
                
                var unconsciousCardsInHand = handPile.Cards.Where(c => 
                    KoishiExtensions.IsTrulyUnconscious(c) && 
                    (c.Keywords == null || !c.Keywords.Contains(CardKeyword.Unplayable)) &&
                    c != this 
                ).ToList();

                if (unconsciousCardsInHand.Count > 0)
                {
                    var targetCard = player.RunState.Rng.Shuffle.NextItem(unconsciousCardsInHand);

                    if (targetCard != null)
                    {
                        var validEnemies = base.CombatState.HittableEnemies.Where(e => !e.IsDead).ToList();
                        Creature? autoTarget = validEnemies.Count > 0 ? player.RunState.Rng.Shuffle.NextItem(validEnemies) : null;

                        KoishiExtensions.AutoPlayedByUnconsciousCards.Add(targetCard);
                        
                        await CardCmd.AutoPlay(choiceContext, targetCard, autoTarget, AutoPlayType.Default, false, false);
                        
                        KoishiExtensions.AutoPlayedByUnconsciousCards.Remove(targetCard);
                    }
                }
            }
            catch (Exception e)
            {
                MegaCrit.Sts2.Core.Logging.Log.Error($"[CasualGame_Koishi] Error: {e.Message}");
            }
        }

        protected override void OnUpgrade()
        {
            base.DynamicVars.Cards.UpgradeValueBy(1m);
        }
    }
}