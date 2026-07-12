using System.Collections.Generic;
using System.Threading.Tasks;
using BaseLib.Abstracts;
using BaseLib.Utils;
using KomeijiKoishi.Cards.Fumo;
using KomeijiKoishi.Enums;
using KomeijiKoishi.Pools;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.Relics;
using MegaCrit.Sts2.Core.Rooms;
using MegaCrit.Sts2.Core.Saves.Runs;

namespace KomeijiKoishi.Relics
{
    [Pool(typeof(KoishiSharedRelicPool))]
    public sealed class FumoCardRelic_Koishi : CustomRelicModel
    {
        private const string TurnsKey = "Turns";
        private const string CardsKey = "Cards";

        private int turnsUntilFumo = 3;

        public override RelicRarity Rarity => RelicRarity.Rare;

        public override bool ShowCounter => true;

        public override int DisplayAmount => this.TurnsUntilFumo;

        protected override IEnumerable<DynamicVar> CanonicalVars => new List<DynamicVar>
        {
            new CardsVar(1),
            new DynamicVar(TurnsKey, 3m)
        };

        protected override IEnumerable<IHoverTip> ExtraHoverTips => new List<IHoverTip>
        {
            HoverTipFactory.FromKeyword(KoishiKeywords.Fumo)
        };

        public override string PackedIconPath => "res://mods/Komeiji_Koishi/images/relics/FumoCardRelic_Koishi.png";
        protected override string PackedIconOutlinePath => "res://mods/Komeiji_Koishi/images/relics/FumoCardRelic_Koishi.png";
        protected override string BigIconPath => "res://mods/Komeiji_Koishi/images/relics/FumoCardRelic_Koishi.png";

        [SavedProperty]
        public int TurnsUntilFumo
        {
            get => this.turnsUntilFumo;
            set
            {
                base.AssertMutable();
                this.turnsUntilFumo = value;
                RefreshStatus();
            }
        }

        public override async Task AfterPlayerTurnStart(PlayerChoiceContext choiceContext, Player player)
        {
            if (player != base.Owner)
            {
                return;
            }

            this.TurnsUntilFumo--;
            if (this.TurnsUntilFumo > 0)
            {
                return;
            }

            CombatState? combatState = player.Creature.CombatState as CombatState;
            if (combatState == null)
            {
                this.TurnsUntilFumo = base.DynamicVars[TurnsKey].IntValue;
                return;
            }

            base.Flash();
            this.TurnsUntilFumo = base.DynamicVars[TurnsKey].IntValue;

            for (int i = 0; i < base.DynamicVars[CardsKey].IntValue; i++)
            {
                await FumoPool.CreateRandomFumoInHand(base.Owner, combatState);
            }
        }

        public override Task AfterCombatEnd(CombatRoom _)
        {
            RefreshStatus();
            return Task.CompletedTask;
        }

        private void RefreshStatus()
        {
            base.Status = this.TurnsUntilFumo == 1
                ? RelicStatus.Active
                : RelicStatus.Normal;
            base.InvokeDisplayAmountChanged();
        }
    }
}
