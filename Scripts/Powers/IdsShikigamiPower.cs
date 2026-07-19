using System.Linq;
using System.Threading.Tasks;
using BaseLib.Abstracts;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Factories;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Cards;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.Models.Powers;

namespace KomeijiKoishi.Powers
{
    public sealed class IdsShikigamiPower : CustomPowerModel
    {
        public override PowerType Type => PowerType.Buff;

        public override PowerStackType StackType => PowerStackType.Single;

        public override string? CustomPackedIconPath => $"res://mods/Komeiji_Koishi/images/powers/IdsShikigamiPower.png";

        public override string? CustomBigIconPath => $"res://mods/Komeiji_Koishi/images/powers/IdsShikigamiPower.png";

        protected override string SmartDescriptionLocKey =>
            base.Id.Entry + (KomeijiKoishi.Config.KoishiBalanceManager.IsEnabled ? ".balanceDescription" : ".smartDescription");

        public override async Task AfterCardPlayed(PlayerChoiceContext choiceContext, CardPlay cardPlay)
        {
            if (cardPlay.Card.Owner?.Creature != base.Owner) return;
            if (cardPlay.Card.Type != CardType.Skill) return;

            Player? player = cardPlay.Card.Owner as Player;
            if (player == null || base.CombatState == null) return;

            this.Flash();

            var skillPool = from c in player.Character.CardPool.GetUnlockedCards(player.UnlockState, player.RunState.CardMultiplayerConstraint)
                            where c.Type == CardType.Skill
                            select c;

            var randomSkill = CardFactory.GetDistinctForCombat(
                player,
                skillPool,
                1,
                player.RunState.Rng.CombatCardGeneration
            ).FirstOrDefault();

            if (randomSkill != null)
            {
                await CardPileCmd.AddGeneratedCardToCombat(
                    randomSkill,
                    PileType.Hand,
                    player,
                    CardPilePosition.Bottom
                );
            }
        }
    }
}