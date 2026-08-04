using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BaseLib.Abstracts;
using BaseLib.Utils;
using KomeijiKoishi.Pools;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.CardPools;
using MegaCrit.Sts2.Core.Models.Cards;

namespace KomeijiKoishi.Cards.Fumo
{
    [Pool(typeof(TokenCardPool))]
    public sealed class MarisaMoonFumo_Koishi : CustomCardModel
    {
        public MarisaMoonFumo_Koishi()
            : base(0, CardType.Skill, CardRarity.Token, TargetType.Self, true)
        {
        }

        public override string PortraitPath => $"res://mods/Komeiji_Koishi/images/cards/fumo/{GetType().Name}.png";

        public override IEnumerable<CardKeyword> CanonicalKeywords => new[] { CardKeyword.Exhaust };

        protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
        {
            if (base.CombatState == null)
            {
                return;
            }

            List<CardModel> cards = PileType.Draw.GetPile(base.Owner).Cards.ToList();
            foreach (CardModel card in cards)
            {
                if (CombatManager.Instance.IsOverOrEnding)
                {
                    break;
                }

                card.ExhaustOnNextPlay = true;
                Creature? target = card.TargetType == TargetType.AnyEnemy
                    ? base.Owner.RunState.Rng.CombatTargets.NextItem(base.CombatState.HittableEnemies)
                    : null;
                await CardCmd.AutoPlay(choiceContext, card, target, AutoPlayType.Default, false, false);
            }
        }

        protected override void OnUpgrade()
        {
            base.EnergyCost.UpgradeBy(+385);
        }
    }
}
