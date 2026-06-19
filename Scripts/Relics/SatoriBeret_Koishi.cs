using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BaseLib.Abstracts;
using BaseLib.Utils;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Creatures; 
using MegaCrit.Sts2.Core.Entities.Relics; 
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Relics; 
using KomeijiKoishi.Pools;
using KomeijiKoishi.Enums; 
using KomeijiKoishi.Utils_Koishi; 
using KomeijiKoishi.Cards;
using KomeijiKoishi.Powers; 
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;

namespace KomeijiKoishi.Relics
{
    [Pool(typeof(KoishiRelicPool))] 
    public sealed class SatoriBeret_Koishi : CustomRelicModel
    {
        public override RelicRarity Rarity => RelicRarity.Rare;

        private bool _usedThisCombat = false;

        protected override IEnumerable<DynamicVar> CanonicalVars => new List<DynamicVar> 
        { 
            new CardsVar(2),
            new EnergyVar(2) 
        };

        protected override IEnumerable<IHoverTip> ExtraHoverTips => new List<IHoverTip> 
        { 
            HoverTipFactory.FromPower<BloomStancePower>(),
            HoverTipFactory.ForEnergy(this)
        };

        public override string PackedIconPath => $"res://mods/Komeiji_Koishi/images/relics/SatoriBeret_Koishi_outline.png";
        protected override string PackedIconOutlinePath => $"res://mods/Komeiji_Koishi/images/relics/SatoriBeret_Koishi.png";
        protected override string BigIconPath => $"res://mods/Komeiji_Koishi/images/relics/SatoriBeret_Koishi.png";

        public override Task AfterSideTurnStart(CombatSide side, IReadOnlyList<Creature> participants, ICombatState combatState)
        {
            if (side == base.Owner.Creature.Side && combatState.RoundNumber <= 1)
            {
                _usedThisCombat = false;
            }
            return Task.CompletedTask;
        }

        public override async Task AfterCardPlayed(PlayerChoiceContext context, CardPlay cardPlay)
        {

            if (cardPlay.Card.Owner == base.Owner && !_usedThisCombat)
            {
                bool hasBloom = base.Owner.Creature.Powers.Any(p => p is BloomStancePower);

                if (hasBloom)
                {
                    _usedThisCombat = true; 
                    
                    base.Flash(); 
                    
                    await CardPileCmd.Draw(context, base.DynamicVars.Cards.BaseValue, base.Owner, false);
                    
                    await PlayerCmd.GainEnergy(base.DynamicVars.Energy.BaseValue, base.Owner);
                }
            }
        }
    }
}