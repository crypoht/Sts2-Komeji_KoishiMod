using System.Threading.Tasks;
using BaseLib.Abstracts;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using KomeijiKoishi.Pools;
using KomeijiKoishi.Powers;
using BaseLib.Utils;
using KomeijiKoishi.Enums;
using MegaCrit.Sts2.Core.HoverTips;
using System.Collections.Generic;
using System.Linq;
using KomeijiKoishi.Utils_Koishi;
using MegaCrit.Sts2.Core.Entities.Creatures;

namespace KomeijiKoishi.Cards
{
    [Pool(typeof(KoishiCardPool))]
    public sealed class PerfectMindControl_Koishi : CustomCardModel,IUseAncientCardFace
    {
        public PerfectMindControl_Koishi() 
            : base(514, CardType.Skill, CardRarity.Uncommon, TargetType.Self, true) { }

        public override string PortraitPath => KoishiImagePaths.CardPortrait(GetType());

        protected override bool HasEnergyCostX => true;

        public override IEnumerable<CardKeyword> CanonicalKeywords => new[] { CardKeyword.Exhaust };

        protected override IEnumerable<IHoverTip> ExtraHoverTips => new[] 
        { 
            HoverTipFactory.FromKeyword(KoishiKeywords.Unconscious) 
        };

        protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
        {
            var player = base.Owner as Player;
            if (player == null) return;

            await CreatureCmd.TriggerAnim(player.Creature, "Cast", player.Character.CastAnimDelay);

            int xValue = KoishiExtensions.AutoPlayedByUnconsciousCards.Contains(this)
                ? (player.PlayerCombatState?.Energy ?? 0)
                : base.ResolveEnergyXValue();
            int amount = xValue + (base.IsUpgraded ? 1 : 0);
            if (amount <= 0) return;

            var handCards = PileType.Hand.GetPile(player)?.Cards?.ToList() ?? new List<CardModel>();
            if (handCards.Count == 0) return;

            var selectedCards = SelectCardsByPriority(player, handCards, amount);

            foreach (var card in selectedCards)
            {
                KoishiExtensions.ApplyUnconsciousToCard(card);
                await KoishiExtensions.SafeAutoPlayCard(choiceContext, player, card);
            }
        }

        protected override void OnUpgrade()
        {
        }

        private static List<CardModel> SelectCardsByPriority(Player player, List<CardModel> cards, int amount)
        {
            var result = new List<CardModel>();
            var remaining = new List<CardModel>(cards);
            AddCardsByPrimaryTypes(player, remaining, result, amount);

            while (result.Count < amount && remaining.Count > 0)
            {
                var selected = player.RunState.Rng.Shuffle.NextItem(remaining);
                if (selected == null) break;

                result.Add(selected);
                remaining.Remove(selected);
            }

            return result;
        }

        private static void AddCardsByPrimaryTypes(
            Player player,
            List<CardModel> remaining,
            List<CardModel> result,
            int amount)
        {
            while (result.Count < amount)
            {
                var group = remaining
                    .Where(c => c.Type == CardType.Skill
                             || c.Type == CardType.Attack
                             || c.Type == CardType.Power)
                    .ToList();
                if (group.Count == 0)
                {
                    return;
                }

                var selected = player.RunState.Rng.Shuffle.NextItem(group);
                if (selected == null)
                {
                    return;
                }

                result.Add(selected);
                remaining.Remove(selected);
            }
        }
    }
}
