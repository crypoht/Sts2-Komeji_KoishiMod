using System.Collections.Generic;
using System.Threading.Tasks;
using BaseLib.Abstracts;
using BaseLib.Utils;
using KomeijiKoishi.Pools;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.Factories;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Relics;
using MegaCrit.Sts2.Core.Rooms;
using MegaCrit.Sts2.Core.Saves.Runs;

namespace KomeijiKoishi.Relics
{
    [Pool(typeof(KoishiSharedRelicPool))]
    public sealed class NuclearManagementManual_Koishi : CustomRelicModel
    {
        private const string ExhaustThresholdKey = "ExhaustThreshold";

        private int _cardsExhausted;

        public override RelicRarity Rarity => RelicRarity.Ancient;

        public override bool ShowCounter => true;

        public override int DisplayAmount => this.CardsExhausted;

        protected override IEnumerable<DynamicVar> CanonicalVars => new List<DynamicVar>
        {
            new CardsVar(1),
            new DynamicVar(ExhaustThresholdKey, 10m)
        };

        public override string PackedIconPath => $"res://mods/Komeiji_Koishi/images/relics/NuclearManagementManual_Koishi.png";
        protected override string PackedIconOutlinePath => $"res://mods/Komeiji_Koishi/images/relics/NuclearManagementManual_Koishi.png";
        protected override string BigIconPath => $"res://mods/Komeiji_Koishi/images/relics/NuclearManagementManual_Koishi.png";

        [SavedProperty]
        public int CardsExhausted
        {
            get => this._cardsExhausted;
            set
            {
                base.AssertMutable();
                this._cardsExhausted = value;
                base.Status = this._cardsExhausted >= base.DynamicVars[ExhaustThresholdKey].IntValue
                    ? RelicStatus.Active
                    : RelicStatus.Normal;
                base.InvokeDisplayAmountChanged();
            }
        }

        public override async Task BeforeSideTurnEnd(PlayerChoiceContext choiceContext, CombatSide side, IEnumerable<Creature> participants)
        {
            if (!participants.Contains(base.Owner.Creature))
            {
                return;
            }

            CardPile hand = PileType.Hand.GetPile(base.Owner);
            CardModel? card = base.Owner.RunState.Rng.CombatCardSelection.NextItem(hand.Cards);
            if (card == null)
            {
                return;
            }

            base.Flash();
            await CardCmd.Exhaust(choiceContext, card, false, false);
            this.CardsExhausted++;
        }

        public override async Task AfterCombatEnd(CombatRoom _)
        {
            int threshold = base.DynamicVars[ExhaustThresholdKey].IntValue;
            if (this.CardsExhausted >= threshold)
            {
                this.CardsExhausted -= threshold;
                base.Flash();
                await RelicCmd.Obtain(RelicFactory.PullNextRelicFromFront(base.Owner).ToMutable(), base.Owner, -1);
            }

            base.Status = RelicStatus.Normal;
            base.InvokeDisplayAmountChanged();
        }
    }
}
