using System.Linq;
using BaseLib.Abstracts;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players; 
using MegaCrit.Sts2.Core.Entities.Powers; 
using MegaCrit.Sts2.Core.Models; 
using MegaCrit.Sts2.Core.Models.Cards; 
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer; 

namespace KomeijiKoishi.Powers
{
    public interface IStanceListenerCard
    {
        void OnStanceChanged(bool isClosedStance, bool isBloomStance);
    }

    public abstract class KoishiStancePower : CustomPowerModel
    {
        public override PowerType Type => PowerType.Buff;
        public override PowerStackType StackType => PowerStackType.Counter;
        
        public static void ClearOldStances(Player player)
        {
            var oldStances = player.Creature.Powers.Where(p => p is KoishiStancePower).ToList();
            bool removedAny = false;

            foreach (var stance in oldStances)
            {
                stance.RemoveInternal();
                removedAny = true;
            }

            if (removedAny)
            {
                NotifyAllCardsStanceChanged(player);
            }
        }

        public static void NotifyAllCardsStanceChanged(Player player, string forceStance = "")
        {
            if (player == null) return;

            bool isClosed = forceStance == "Closed";
            bool isBloom = forceStance == "Bloom";

            if (forceStance == "")
            {
                isClosed = player.Creature.Powers.Any(p => p is ClosedStancePower);
                isBloom = player.Creature.Powers.Any(p => p is BloomStancePower);
            }

            var allPiles = new[] { 
                PileType.Hand.GetPile(player), 
                PileType.Draw.GetPile(player), 
                PileType.Discard.GetPile(player) 
            };

            foreach (var pile in allPiles)
            {
                if (pile == null || pile.Cards == null) continue;
                foreach (var card in pile.Cards)
                {
                    if (card is IStanceListenerCard listenerCard)
                    {
                        listenerCard.OnStanceChanged(isClosed, isBloom);
                    }
                }
            }
        }
    }
}