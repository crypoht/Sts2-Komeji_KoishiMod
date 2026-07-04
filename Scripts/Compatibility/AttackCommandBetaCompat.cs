using MegaCrit.Sts2.Core.Commands.Builders;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Models;

namespace MegaCrit.Sts2.Core.Commands
{
    public static class AttackCommandBetaCompat
    {
#if STS2_BETA
        public static AttackCommand FromCard(this AttackCommand command, CardModel card)
        {
            return command.FromCard(card, null);
        }
#else
        public static AttackCommand FromCard(this AttackCommand command, CardModel card, CardPlay? cardPlay)
        {
            return command.FromCard(card);
        }
#endif
    }
}
