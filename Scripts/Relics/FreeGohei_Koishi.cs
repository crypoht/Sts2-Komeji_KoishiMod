using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BaseLib.Abstracts;
using BaseLib.Utils;
using KomeijiKoishi.Pools;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Relics;
using MegaCrit.Sts2.Core.Rooms;

namespace KomeijiKoishi.Relics
{
    [Pool(typeof(KoishiSharedRelicPool))]
    public sealed class FreeGohei_Koishi : CustomRelicModel
    {
        private const string CostThresholdKey = "CostThreshold";
        private const string AttackLimitKey = "AttackLimit";

        private int _attacksPlayedThisTurn;

        public override RelicRarity Rarity => RelicRarity.Ancient;

        public override bool ShowCounter => CombatManager.Instance.IsInProgress;

        public override int DisplayAmount => this.AttacksPlayedThisTurn;

        protected override IEnumerable<DynamicVar> CanonicalVars => new List<DynamicVar>
        {
            new DynamicVar(CostThresholdKey, 2m),
            new DynamicVar(AttackLimitKey, 3m)
        };

        public override string PackedIconPath => "res://mods/Komeiji_Koishi/images/relics/FreeGohei_Koishi.png";
        protected override string PackedIconOutlinePath => "res://mods/Komeiji_Koishi/images/relics/FreeGohei_Koishi.png";
        protected override string BigIconPath => "res://mods/Komeiji_Koishi/images/relics/FreeGohei_Koishi.png";

        private int AttacksPlayedThisTurn
        {
            get => this._attacksPlayedThisTurn;
            set
            {
                base.AssertMutable();
                this._attacksPlayedThisTurn = value;
                this.RefreshCounter();
            }
        }

        public override bool TryModifyEnergyCostInCombat(CardModel card, decimal originalCost, out decimal modifiedCost)
        {
            if (card.Owner != base.Owner || card.EnergyCost.CostsX || originalCost < base.DynamicVars[CostThresholdKey].BaseValue)
            {
                modifiedCost = originalCost;
                return false;
            }

            modifiedCost = originalCost - 1m;
            return true;
        }

        public override bool ShouldPlay(CardModel card, AutoPlayType autoPlayType)
        {
            if (card.Owner != base.Owner || card.Type != CardType.Attack)
            {
                return true;
            }

            return this.AttacksPlayedThisTurn < base.DynamicVars[AttackLimitKey].IntValue;
        }

        public override Task AfterCardPlayed(PlayerChoiceContext choiceContext, CardPlay cardPlay)
        {
            if (cardPlay.Card.Owner == base.Owner && cardPlay.Card.Type == CardType.Attack)
            {
                this.AttacksPlayedThisTurn++;
            }

            return Task.CompletedTask;
        }

        public override Task AfterSideTurnEnd(PlayerChoiceContext choiceContext, CombatSide side, IEnumerable<Creature> participants)
        {
            if (participants.Contains(base.Owner.Creature))
            {
                this.AttacksPlayedThisTurn = 0;
            }

            return Task.CompletedTask;
        }

        public override Task AfterCombatEnd(CombatRoom _)
        {
            this.AttacksPlayedThisTurn = 0;
            base.Status = RelicStatus.Normal;
            base.InvokeDisplayAmountChanged();
            return Task.CompletedTask;
        }

        private void RefreshCounter()
        {
            base.Status = this.AttacksPlayedThisTurn >= base.DynamicVars[AttackLimitKey].IntValue
                ? RelicStatus.Active
                : RelicStatus.Normal;
            base.InvokeDisplayAmountChanged();
        }
    }
}
