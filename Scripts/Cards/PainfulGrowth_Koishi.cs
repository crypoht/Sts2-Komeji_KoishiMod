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
using KomeijiKoishi.Pools;
using KomeijiKoishi.Utils_Koishi;
using KomeijiKoishi.Enums;
using MegaCrit.Sts2.Core.HoverTips; 

namespace KomeijiKoishi.Cards
{
    [Pool(typeof(KoishiCardPool))]
    public sealed class PainfulGrowth_Koishi : CustomCardModel
    {
        public PainfulGrowth_Koishi() 
            : base(2, CardType.Skill, CardRarity.Uncommon, TargetType.Self, true) { }

        public override string PortraitPath => $"res://mods/Komeiji_Koishi/images/cards/{GetType().Name}.png";

        protected override IEnumerable<IHoverTip> ExtraHoverTips => new[] 
        { 
            HoverTipFactory.FromKeyword(KoishiKeywords.Unconscious) 
        };
        protected override IEnumerable<DynamicVar> CanonicalVars => new List<DynamicVar> 
        { 
            new DynamicVar("Amount", 3m) 
        };

        protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
        {
            try
            {
                var player = base.Owner as MegaCrit.Sts2.Core.Entities.Players.Player;
                if (player == null) return;

                var handPile = PileType.Hand.GetPile(player);
                
                var unconsciousCards = handPile.Cards.Where(c => 
                    KoishiExtensions.IsTrulyUnconscious(c) && 
                    (c.Keywords == null || !c.Keywords.Contains(CardKeyword.Unplayable)) &&
                    c != this
                ).ToList();

                int countToPlay = (int)Math.Min(base.DynamicVars["Amount"].BaseValue, unconsciousCards.Count);

                for (int i = 0; i < countToPlay; i++)
                {
                    if (base.CombatState == null || base.CombatState.HittableEnemies.All(e => e.IsDead)) break;

                    var targetCard = player.RunState.Rng.Shuffle.NextItem(unconsciousCards);
                    if (targetCard != null)
                    {
                        unconsciousCards.Remove(targetCard);

                        var validEnemies = base.CombatState.HittableEnemies.Where(e => !e.IsDead).ToList();
                        Creature? autoTarget = validEnemies.Count > 0 ? player.RunState.Rng.Shuffle.NextItem(validEnemies) : null;

                        await CardCmd.AutoPlay(choiceContext, targetCard, autoTarget, AutoPlayType.Default, true, false);
                        
                        await Cmd.Wait(0.1f, false);
                    }
                }
            }
            catch (Exception e)
            {
                MegaCrit.Sts2.Core.Logging.Log.Error($"[PainfulGrowth_Koishi] Error: {e.Message}");
            }
        }

        protected override void OnUpgrade()
        {
            base.DynamicVars["Amount"].UpgradeValueBy(1m);
        }
    }
}