using System.Collections.Generic;
using System.Threading.Tasks;
using BaseLib.Abstracts;
using MegaCrit.Sts2.Core.CardSelection;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Relics; 
using MegaCrit.Sts2.Core.Nodes;
using MegaCrit.Sts2.Core.Nodes.Vfx;
using MegaCrit.Sts2.Core.Extensions; 
using KomeijiKoishi.Pools;
using KomeijiKoishi.Enchantments;
using BaseLib.Utils;
using System;
using System.Linq;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Creatures; 
using MegaCrit.Sts2.Core.Entities.Relics; 
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using KomeijiKoishi.Enums; 
using KomeijiKoishi.Utils_Koishi; 
using KomeijiKoishi.Cards;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.ValueProps;
using MegaCrit.Sts2.Core.Saves.Runs;

namespace KomeijiKoishi.Relics
{
    [Pool(typeof(KoishiRelicPool))] 
    public sealed class OkuusSecretEye_Koishi : CustomRelicModel
    {
        public override RelicRarity Rarity => RelicRarity.Rare;

        public override bool ShowCounter => true;

        protected override IEnumerable<DynamicVar> CanonicalVars => new List<DynamicVar> 
        { 
            new EnergyVar(10),
            new DamageVar(25m, ValueProp.Unpowered) 
        };

        public override string PackedIconPath => $"res://mods/Komeiji_Koishi/images/relics/OkuusSecretEye_Koishi_outline.png";
        protected override string PackedIconOutlinePath => $"res://mods/Komeiji_Koishi/images/relics/OkuusSecretEye_Koishi.png";
        protected override string BigIconPath => $"res://mods/Komeiji_Koishi/images/relics/OkuusSecretEye_Koishi.png";
        private bool _isActivating;

        private bool IsActivating
        {
            get => this._isActivating;
            set
            {
                base.AssertMutable();
                this._isActivating = value;
                this.UpdateDisplay();
            }
        }

        private int _energySpent;

        [SavedProperty]
        public int EnergySpent
        {
            get => this._energySpent;
            set
            {
                base.AssertMutable();
                this._energySpent = value;
                this.UpdateDisplay();
            }
        }

        private int _pendingDamageTriggers;

        public override int DisplayAmount
        {
            get
            {
                if (!this.IsActivating)
                {
                    return this.EnergySpent % base.DynamicVars.Energy.IntValue;
                }
                return base.DynamicVars.Energy.IntValue;
            }
        }

        private void UpdateDisplay()
        {
            if (this.IsActivating)
            {
                base.Status = RelicStatus.Normal;
            }
            else
            {
                int threshold = base.DynamicVars.Energy.IntValue;
                base.Status = ((this.EnergySpent % threshold == threshold - 1) ? RelicStatus.Active : RelicStatus.Normal);
            }
            base.InvokeDisplayAmountChanged();
        }

        public override Task AfterEnergySpent(CardModel card, int amount)
        {
            if (card.Owner == base.Owner && amount > 0)
            {
                int threshold = base.DynamicVars.Energy.IntValue;
                
                int prevTriggers = this.EnergySpent / threshold;
                this.EnergySpent += amount;
                int newTriggers = this.EnergySpent / threshold;
                
                int triggersToExecute = newTriggers - prevTriggers;

                if (triggersToExecute > 0)
                {
                    this._pendingDamageTriggers += triggersToExecute;
                }
            }
            
            return Task.CompletedTask;
        }

        public override async Task AfterCardPlayed(PlayerChoiceContext context, CardPlay cardPlay)
        {
            if (this._pendingDamageTriggers > 0 && CombatManager.Instance.IsInProgress)
            {
                for (int i = 0; i < this._pendingDamageTriggers; i++)
                {
                    var enemies = base.Owner?.Creature?.CombatState?.HittableEnemies;
                    if (enemies != null && enemies.Count > 0)
                    {
                        var creature = base.Owner!.RunState.Rng.CombatTargets.NextItem<Creature>(enemies);
                        if (creature != null)
                        {
                            _ = TaskHelper.RunSafely(this.DoActivateVisuals());
                            
                            await CreatureCmd.Damage(context, creature, base.DynamicVars.Damage, base.Owner.Creature);
                        }
                    }
                }

                this._pendingDamageTriggers = 0;
            }
        }

        private async Task DoActivateVisuals()
        {
            this.IsActivating = true;
            base.Flash();
            await Cmd.Wait(0.2f, false); 
            this.IsActivating = false;
        }
    }
}