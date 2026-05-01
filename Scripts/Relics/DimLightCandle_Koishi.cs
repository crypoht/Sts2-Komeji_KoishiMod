using System.Collections.Generic;
using System.Threading.Tasks;
using BaseLib.Abstracts;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards; 
using MegaCrit.Sts2.Core.GameActions.Multiplayer; 
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.Relics;
using KomeijiKoishi.Pools;
using BaseLib.Utils;
using MegaCrit.Sts2.Core.Entities.Creatures; 
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.Saves.Runs;
using MegaCrit.Sts2.Core.Models;

namespace KomeijiKoishi.Relics
{
    [Pool(typeof(KoishiSharedRelicPool))]
    public sealed class DimLightCandle_Koishi : CustomRelicModel
    {
        public override RelicRarity Rarity => RelicRarity.Uncommon;


        protected override IEnumerable<DynamicVar> CanonicalVars => new List<DynamicVar> 
        { 
            new HealVar(1m) 
        };

        public override bool ShowCounter => true;

        public override string PackedIconPath => $"res://mods/Komeiji_Koishi/images/relics/DimLightCandle_Koishi.png";
        protected override string PackedIconOutlinePath => $"res://mods/Komeiji_Koishi/images/relics/DimLightCandle_Koishi.png";
        protected override string BigIconPath => $"res://mods/Komeiji_Koishi/images/relics/DimLightCandle_Koishi.png";

        private int _triggerCountThisCombat;

        [SavedProperty]
        public int TriggerCountThisCombat
        {
            get => this._triggerCountThisCombat;
            set
            {
                base.AssertMutable();
                this._triggerCountThisCombat = value;
                base.InvokeDisplayAmountChanged();
            }
        }


        public override int DisplayAmount => 6 - this.TriggerCountThisCombat;


        public override Task BeforeCombatStart()
        {
            this.TriggerCountThisCombat = 0;
            base.Status = RelicStatus.Active; 
            return base.BeforeCombatStart();
        }

        public override async Task AfterCardExhausted(PlayerChoiceContext choiceContext, CardModel card, bool _)
        {

            if (card.Owner == base.Owner && this.TriggerCountThisCombat < 6)
            {
                base.Flash();
                

                this.TriggerCountThisCombat++;

                if (this.TriggerCountThisCombat >= 6)
                {
                    base.Status = RelicStatus.Normal;
                }

                await CreatureCmd.Heal(base.Owner.Creature, base.DynamicVars.Heal.BaseValue, true);
            }
        }
    }
}