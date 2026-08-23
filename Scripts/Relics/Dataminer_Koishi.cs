using BaseLib.Abstracts;
using BaseLib.Utils;
using KomeijiKoishi.Cards;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using KomeijiKoishi.Pools;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Relics;
using MegaCrit.Sts2.Core.Saves.Runs;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Linq;

namespace KomeijiKoishi.Relics
{
    [Pool(typeof(KoishiSharedRelicPool))]
    public sealed class Dataminer_Koishi : CustomRelicModel
    {
        private int _cooldown;

        public override RelicRarity Rarity => RelicRarity.Rare;

        public override bool ShowCounter => Cooldown > 0;
        public override int DisplayAmount => Cooldown;

        [SavedProperty]
        public int Cooldown
        {
            get => _cooldown;
            private set
            {
                base.AssertMutable();
                _cooldown = Math.Max(0, value);
                base.Status = _cooldown == 0 ? RelicStatus.Active : RelicStatus.Normal;
                base.InvokeDisplayAmountChanged();
            }
        }

        public bool CanTrigger => Cooldown == 0;

        public void StartCooldown()
        {
            Cooldown = 4;
        }

        public override Task AfterPlayerTurnStart(PlayerChoiceContext choiceContext, Player player)
        {
            if (player == base.Owner && Cooldown > 0)
            {
                Cooldown--;
            }

            return Task.CompletedTask;
        }

        public override async Task BeforeSideTurnEnd(
            PlayerChoiceContext choiceContext,
            CombatSide side,
            IEnumerable<Creature> participants)
        {
            if (base.Owner == null || side != base.Owner.Creature.Side)
            {
                return;
            }

            Player? player = base.Owner;
            List<DataminerCard_Koishi> cards = PileType.Hand.GetPile(player).Cards
                .OfType<DataminerCard_Koishi>()
                .Where(card => card.AutoPlayAtTurnEnd)
                .ToList();

            foreach (DataminerCard_Koishi card in cards)
            {
                if (card.Pile == null || card.Pile.Type != PileType.Hand)
                {
                    continue;
                }

                await CardCmd.AutoPlay(
                    choiceContext,
                    card,
                    null,
                    AutoPlayType.Default,
                    false,
                    false);
            }
        }

        public override string PackedIconPath => "res://mods/Komeiji_Koishi/images/relics/Dataminer.png";
        protected override string PackedIconOutlinePath => "res://mods/Komeiji_Koishi/images/relics/Dataminer.png";
        protected override string BigIconPath => "res://mods/Komeiji_Koishi/images/relics/Dataminer.png";
    }
}
