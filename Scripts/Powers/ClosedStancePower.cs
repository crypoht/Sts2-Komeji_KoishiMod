using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players; 
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.ValueProps;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;

namespace KomeijiKoishi.Powers
{
    public sealed class ClosedStancePower : KoishiStancePower
    {
        public override string? CustomPackedIconPath => $"res://mods/Komeiji_Koishi/images/powers/ClosedStancePower.png";
        public override string? CustomBigIconPath => $"res://mods/Komeiji_Koishi/images/powers/ClosedStancePower.png";
       public static async Task EnterThisStance(PlayerChoiceContext context, Player player, CardModel sourceCard)
        {
            try 
            {
                if (player.Creature.GetPower<ClosedStancePower>() != null) return;
                
                await ClearOldStances(player); 

                decimal drawAmount = 1m;
                var superego = player.Creature.Powers.FirstOrDefault(p => p is SuperegoPower);
                if (superego != null) drawAmount += superego.Amount;

                await CardPileCmd.Draw(context, drawAmount, player, false);
                await PowerCmd.Apply<ClosedStancePower>(player.Creature, 1m, player.Creature, sourceCard, false);
                
                await NotifyAllCardsStanceChanged(player, "Closed"); 

                await NotifyAllPowersStanceChanged(context, player, "Closed", sourceCard);
            }
            catch (Exception e)
            {
                MegaCrit.Sts2.Core.Logging.Log.Error($"[ClosedStance] EnterThisStance 严重报错: {e}");
            }
        }

        public decimal BlockBonus 
        {
            get 
            {
                decimal bonus = 20m; 
                var rosePower = base.Owner?.GetPower<RoseProtectionPower>();
                if (rosePower != null)
                {
                    bonus += rosePower.Amount;
                }
                return bonus;
            }
        }

        public override decimal ModifyBlockMultiplicative(Creature target, decimal block, ValueProp props, CardModel? cardSource, CardPlay? cardPlay)
        {
            if (target == base.Owner)
            {
                return 1m + (BlockBonus / 100m);
            }
            return 1m;
        }
    }
}