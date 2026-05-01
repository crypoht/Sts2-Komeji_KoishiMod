using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using BaseLib.Abstracts;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Models.Relics;
using KomeijiKoishi.Pools;
using KomeijiKoishi.Utils_Koishi; 
using BaseLib.Utils;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.Models;


namespace KomeijiKoishi.Relics
{
    [Pool(typeof(KoishiRelicPool))]
    public sealed class IdolsMicrophone_Koishi : CustomRelicModel
    {
        public override RelicRarity Rarity => RelicRarity.Shop;

        public override string PackedIconPath => $"res://mods/Komeiji_Koishi/images/relics/IdolsMicrophone_Koishi_outline.png";
        protected override string PackedIconOutlinePath => $"res://mods/Komeiji_Koishi/images/relics/IdolsMicrophone_Koishi.png";
        protected override string BigIconPath => $"res://mods/Komeiji_Koishi/images/relics/IdolsMicrophone_Koishi.png";

        private bool _usedThisTurn = false;

        public override Task BeforeCombatStart()
        {
            _usedThisTurn = false;
            base.Status = RelicStatus.Active; 
            return base.BeforeCombatStart();
        }

        public override Task AfterSideTurnStart(CombatSide side, CombatState combatState)
        {
            if (side == base.Owner.Creature.Side)
            {
                _usedThisTurn = false;
                base.Status = RelicStatus.Active; 
            }
            return Task.CompletedTask;
        }

        public override ValueTuple<PileType, CardPilePosition> ModifyCardPlayResultPileTypeAndPosition(CardModel card, bool isAutoPlay, ResourceInfo resources, PileType pileType, CardPilePosition position)
        {
            if (!_usedThisTurn && card.Owner == base.Owner && KoishiExtensions.IsTrulyUnconscious(card))
            {
                return new ValueTuple<PileType, CardPilePosition>(PileType.Hand, CardPilePosition.Top);
            }
            
            return new ValueTuple<PileType, CardPilePosition>(pileType, position);
        }

        public override Task AfterModifyingCardPlayResultPileOrPosition(CardModel card, PileType pileType, CardPilePosition position)
        {
            if (!_usedThisTurn && card.Owner == base.Owner && KoishiExtensions.IsTrulyUnconscious(card))
            {
                base.Flash(); 
                _usedThisTurn = true; 
                base.Status = RelicStatus.Normal; 
            }
            return Task.CompletedTask;
        }
    }
}