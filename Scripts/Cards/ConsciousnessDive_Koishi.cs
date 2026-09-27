using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BaseLib.Abstracts;
using BaseLib.Utils;
using KomeijiKoishi.Pools;
using KomeijiKoishi.Utils_Koishi;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;

namespace KomeijiKoishi.Cards
{
    [Pool(typeof(KoishiCardPool))]
    public sealed class ConsciousnessDive_Koishi : CustomCardModel,IUseAncientCardFace
    {
        private static readonly HashSet<Player> ResolvingPlayers = new();

        public ConsciousnessDive_Koishi()
            : base(2, CardType.Skill, CardRarity.Uncommon, TargetType.AnyAlly, true)
        {
        }

        public override string PortraitPath => KoishiImagePaths.CardPortrait(GetType());

        public override IEnumerable<CardKeyword> CanonicalKeywords => new[] { CardKeyword.Exhaust };

        public override CardMultiplayerConstraint MultiplayerConstraint => CardMultiplayerConstraint.MultiplayerOnly;

        protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
        {
            if (cardPlay.Target?.Player == null)
            {
                return;
            }

            Player targetPlayer = cardPlay.Target.Player;
            if (!ResolvingPlayers.Add(targetPlayer))
            {
                return;
            }

            try
            {
                await CreatureCmd.TriggerAnim(base.Owner.Creature, "Cast", base.Owner.Character.CastAnimDelay);

                var cardsToPlay = PileType.Exhaust.GetPile(targetPlayer)
                    .Cards
                    .Where(card => (card.Type == CardType.Attack || card.Type == CardType.Skill)
                        && card is not ConsciousnessDive_Koishi)
                    .ToList();

                foreach (CardModel card in cardsToPlay)
                {
                    Creature? target = GetAutoTarget(targetPlayer, card);
                    await KoishiExtensions.SafeAutoPlayCard(choiceContext, targetPlayer, card, target, AutoPlayType.Default, true, false);
                    await Cmd.Wait(0.05f, false);
                }
            }
            finally
            {
                ResolvingPlayers.Remove(targetPlayer);
            }
        }

        protected override void OnUpgrade()
        {
            base.EnergyCost.UpgradeBy(-1);
        }

        private Creature? GetAutoTarget(Player player, CardModel card)
        {
            if (player.Creature.CombatState == null)
            {
                return null;
            }

            if (card.TargetType == TargetType.AnyEnemy)
            {
                var enemies = player.Creature.CombatState.HittableEnemies.Where(e => e != null && !e.IsDead).ToList();
                return enemies.Count > 0 ? player.RunState.Rng.Shuffle.NextItem(enemies) : null;
            }

            if (card.TargetType == TargetType.AnyAlly)
            {
                var allies = player.Creature.CombatState.Players
                    .Where(p => p != null && p != player && p.Creature != null && !p.Creature.IsDead && p.Creature.Side == player.Creature.Side)
                    .Select(p => p.Creature)
                    .ToList();

                return allies.Count > 0 ? player.RunState.Rng.Shuffle.NextItem(allies) : null;
            }

            if (card.TargetType == TargetType.AnyPlayer)
            {
                return player.Creature;
            }

            return null;
        }
    }
}
