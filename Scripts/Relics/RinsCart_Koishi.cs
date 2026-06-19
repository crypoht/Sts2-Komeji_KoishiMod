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

namespace KomeijiKoishi.Relics
{
    [Pool(typeof(KoishiRelicPool))] 
    public sealed class RinsCart_Koishi : CustomRelicModel
    {

        public override RelicRarity Rarity => RelicRarity.Uncommon;

        protected override IEnumerable<DynamicVar> CanonicalVars => new List<DynamicVar> 
        { 
            new CardsVar(2)
        };

        public override string PackedIconPath => $"res://mods/Komeiji_Koishi/images/relics/RinsCart_Koishi_outline.png";
        protected override string PackedIconOutlinePath => $"res://mods/Komeiji_Koishi/images/relics/RinsCart_Koishi.png";
        protected override string BigIconPath => $"res://mods/Komeiji_Koishi/images/relics/RinsCart_Koishi.png";

        public override async Task AfterDeath(PlayerChoiceContext choiceContext, Creature target, bool wasRemovalPrevented, float deathAnimLength)
        {
                base.Flash(); 
                await CardPileCmd.Draw(choiceContext, base.DynamicVars.Cards.BaseValue, base.Owner, false);
        }
    }
}