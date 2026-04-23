using System;
using System.Collections.Generic;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.ValueProps;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BaseLib.Utils;
using BaseLib.Abstracts; 
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures; 
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using KomeijiKoishi.Enums;

namespace KomeijiKoishi.Powers
{
    public sealed class FlawOfDNAPower : CustomPowerModel
    {
        public override PowerType Type => PowerType.Buff;
        

        public override PowerStackType StackType => PowerStackType.Single;
        
        public override string? CustomPackedIconPath => $"res://mods/Komeiji_Koishi/images/powers/FlawOfDNAPower.png";
        public override string? CustomBigIconPath => $"res://mods/Komeiji_Koishi/images/powers/FlawOfDNAPower.png";


        protected override IEnumerable<IHoverTip> ExtraHoverTips => new List<IHoverTip>
        {
            HoverTipFactory.FromKeyword(CardKeyword.Exhaust)
        };

        public override bool TryModifyEnergyCostInCombat(CardModel card, decimal originalCost, out decimal modifiedCost)
        {
            if (card.Owner?.Creature != base.Owner || card.Type != CardType.Attack)
            {
                modifiedCost = originalCost;
                return false;
            }
            
            modifiedCost = 0m;
            return true;
        }

        public override (PileType, CardPilePosition) ModifyCardPlayResultPileTypeAndPosition(CardModel card, bool isAutoPlay, ResourceInfo resources, PileType pileType, CardPilePosition position)
        {
            if (card.Owner?.Creature != base.Owner || card.Type != CardType.Attack)
            {
                return (pileType, position);
            }
            
            return (PileType.Exhaust, position);
        }
    }
}