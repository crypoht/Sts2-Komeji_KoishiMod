using System.Linq;
using BaseLib.Abstracts;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models.Powers;
using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures; 
using KomeijiKoishi.Pools;
using MegaCrit.Sts2.Core.Entities.Powers; 
using MegaCrit.Sts2.Core.Models; 
using MegaCrit.Sts2.Core.Models.Cards; 

namespace KomeijiKoishi.Powers
{
    public abstract class KoishiStancePower : CustomPowerModel
    {
        public override PowerType Type => PowerType.Buff;
        public override PowerStackType StackType => PowerStackType.None;

        // 广播给所有卡牌
        protected static void NotifyAllCardsStanceChanged(Player player, string stanceName)
        {
            bool isClosedStance = stanceName == "Closed";
            bool isBloomStance = stanceName == "Bloom";

            var allPiles = new[] 
            {
                PileType.Hand.GetPile(player),
                PileType.Draw.GetPile(player),
                PileType.Discard.GetPile(player),
                PileType.Exhaust.GetPile(player)
            };

            foreach (var pile in allPiles)
            {
                if (pile == null) continue; 
                
                foreach (var card in pile.Cards)
                {
                    if (card is IStanceListenerCard listenerCard)
                    {
                        listenerCard.OnStanceChanged(isClosedStance, isBloomStance);
                    }
                }
            }
        }

        protected static async Task NotifyAllPowersStanceChanged(PlayerChoiceContext context, Player player, string stanceName, CardModel? sourceCard)
        {
            bool isClosedStance = stanceName == "Closed";
            bool isBloomStance = stanceName == "Bloom";

            foreach (var power in player.Creature.Powers.ToList()) 
            {
                if (power is IStanceListenerPower listenerPower)
                {
                    try {
                        await listenerPower.OnStanceChanged(isClosedStance, isBloomStance, context, sourceCard);
                    }
                    catch (Exception ex) {
                        MegaCrit.Sts2.Core.Logging.Log.Error($"[排查专用] 广播崩溃的完整真凶：\n{ex.ToString()}");
                    }
                }
            }
        }

        protected static async Task ClearOldStances(Player player)
        {
            var powersToRemove = player.Creature.Powers
                .Where(p => p is BloomStancePower || p is ClosedStancePower)
                .ToList();

            foreach (var power in powersToRemove)
            {
                await PowerCmd.Remove(power); 
            }
        }
    }

    public interface IStanceListenerCard
    {
        void OnStanceChanged(bool isClosedStance, bool isBloomStance);
    }

    public interface IStanceListenerPower
    {
        Task OnStanceChanged(bool isClosedStance, bool isBloomStance, PlayerChoiceContext context, CardModel? sourceCard);
    }
}