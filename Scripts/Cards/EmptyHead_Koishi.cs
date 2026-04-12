using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection; 
using System.Threading.Tasks;
using BaseLib.Abstracts;
using BaseLib.Utils;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Players; 
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.ValueProps;
using KomeijiKoishi.Pools;
using KomeijiKoishi.Enums;

namespace KomeijiKoishi.Cards
{
    [Pool(typeof(KoishiCardPool))]
    public sealed class EmptyHead_Koishi : CustomCardModel
    {
        public EmptyHead_Koishi() 
            : base(1, CardType.Skill, CardRarity.Common, TargetType.Self, true) { }

        public override string PortraitPath => $"res://mods/Komeiji_Koishi/images/cards/{GetType().Name}.png";
        protected override IEnumerable<IHoverTip> ExtraHoverTips => new[] 
        { 
            HoverTipFactory.FromKeyword(KoishiKeywords.Unconscious) 
        };

        protected override IEnumerable<DynamicVar> CanonicalVars => new List<DynamicVar> 
        { 
            new BlockVar(7m, ValueProp.Move) 
        };

        protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
        {
            try
            {
                var player = base.Owner as Player;
                if (player == null) return;

                await CreatureCmd.GainBlock(player.Creature, base.DynamicVars.Block.BaseValue, ValueProp.Move, cardPlay, false);

                var handPile = PileType.Hand.GetPile(player);
                
                var validCards = handPile.Cards.Where(c => 
                    c != this && 
                    (c.Keywords == null || !c.Keywords.Contains(KoishiKeywords.Unconscious))
                ).ToList();

                if (validCards.Count > 0)
                {

                    var targetCard = player.RunState.Rng.Shuffle.NextItem(validCards);

                    if (targetCard != null)
                    {
                        var applyMethod = typeof(CardCmd).GetMethod(
                            "ApplyKeyword", 
                            BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic
                        );

                        if (applyMethod != null)
                        {
                            applyMethod.Invoke(null, new object[] { 
                                targetCard, 
                                new CardKeyword[] { KoishiKeywords.Unconscious } 
                            });
                        }
                    }
                }
            }
            catch (Exception e)
            {
                MegaCrit.Sts2.Core.Logging.Log.Error($"[EmptyHead_Koishi] Error: {e.Message}");
            }
        }

        protected override void OnUpgrade()
        {
            base.DynamicVars.Block.UpgradeValueBy(3m);
        }
    }
}