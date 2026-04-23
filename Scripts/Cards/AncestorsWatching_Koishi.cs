using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using BaseLib.Abstracts;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.Cards;
using KomeijiKoishi.Pools;
using KomeijiKoishi.Powers;
using BaseLib.Utils;

namespace KomeijiKoishi.Cards
{
    [Pool(typeof(KoishiCardPool))]
    public sealed class AncestorsWatching_Koishi : CustomCardModel
    {
        public AncestorsWatching_Koishi() 
            : base(1, CardType.Skill, CardRarity.Rare, TargetType.Self, true) 
        { 
        }

        public override string PortraitPath => $"res://mods/Komeiji_Koishi/images/cards/{GetType().Name}.png";

        public override IEnumerable<CardKeyword> CanonicalKeywords => new[] { CardKeyword.Exhaust };

        protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
        {
            var player = base.Owner as Player;
            if (player == null || base.CombatState == null) return;

            await CreatureCmd.TriggerAnim(player.Creature, "Buff", player.Character!.CastAnimDelay);

            var exhaustPile = PileType.Exhaust.GetPile(player);
            if (exhaustPile != null)
            {
                int exhaustCount = exhaustPile.Cards.Count;
                
                int kuugaAmount = exhaustCount / 2;

                if (kuugaAmount > 0)
                {
                    await PowerCmd.Apply<KuugaPower>(player.Creature, (decimal)kuugaAmount, player.Creature, this, false);
                }
            }
        }

        protected override void OnUpgrade()
        {
            base.EnergyCost.UpgradeBy(-1);
        }
    }
}