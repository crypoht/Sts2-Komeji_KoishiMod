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
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.Relics;
using MegaCrit.Sts2.Core.Rooms;

namespace KomeijiKoishi.Relics
{
    [Pool(typeof(KoishiSharedRelicPool))]
    public sealed class FrogClothes_Koishi : CustomRelicModel
    {
        private int _cardsPlayedThisTurn;

        public override RelicRarity Rarity => RelicRarity.Ancient;

        public override bool ShowCounter => CombatManager.Instance.IsInProgress;

        public override int DisplayAmount => this.CardsPlayedThisTurn;

        protected override IEnumerable<DynamicVar> CanonicalVars => new List<DynamicVar>
        {
            new HealVar(4m)
        };

        public override string PackedIconPath => $"res://mods/Komeiji_Koishi/images/relics/FrogClothes_Koishi.png";
        protected override string PackedIconOutlinePath => $"res://mods/Komeiji_Koishi/images/relics/FrogClothes_Koishi.png";
        protected override string BigIconPath => $"res://mods/Komeiji_Koishi/images/relics/FrogClothes_Koishi.png";

        public int CardsPlayedThisTurn
        {
            get => this._cardsPlayedThisTurn;
            set
            {
                base.AssertMutable();
                this._cardsPlayedThisTurn = value;
                this.RefreshCounter();
            }
        }

        public override Task AfterCardPlayed(PlayerChoiceContext choiceContext, CardPlay cardPlay)
        {
            if (cardPlay.Card.Owner != base.Owner)
            {
                return Task.CompletedTask;
            }

            this.CardsPlayedThisTurn++;
            return Task.CompletedTask;
        }

        public override async Task BeforeSideTurnEnd(PlayerChoiceContext choiceContext, CombatSide side, IEnumerable<Creature> participants)
        {
            if (!participants.Contains(base.Owner.Creature))
            {
                return;
            }

            if (this.CardsPlayedThisTurn == 0)
            {
                base.Flash();
                await CreatureCmd.Heal(base.Owner.Creature, base.DynamicVars.Heal.BaseValue, true);
            }

            this.CardsPlayedThisTurn = 0;
        }

        public override Task AfterCombatEnd(CombatRoom _)
        {
            this.CardsPlayedThisTurn = 0;
            base.Status = RelicStatus.Normal;
            base.InvokeDisplayAmountChanged();
            return Task.CompletedTask;
        }

        private void RefreshCounter()
        {
            base.Status = this.CardsPlayedThisTurn == 0 ? RelicStatus.Active : RelicStatus.Normal;
            base.InvokeDisplayAmountChanged();
        }
    }
}
