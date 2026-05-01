using System.Collections.Generic;
using System.Threading.Tasks;
using BaseLib.Abstracts;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Entities.Players; 
using MegaCrit.Sts2.Core.GameActions.Multiplayer; 
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.Relics;
using KomeijiKoishi.Pools;
using KomeijiKoishi.Cards.Fumo;
using BaseLib.Utils;
using MegaCrit.Sts2.Core.Entities.Relics; 
using KomeijiKoishi.Enums;
using MegaCrit.Sts2.Core.HoverTips;

namespace KomeijiKoishi.Relics
{
    [Pool(typeof(KoishiRelicPool))] 
    public sealed class FumoCardRelic_Koishi : CustomRelicModel
    {
        public override RelicRarity Rarity => RelicRarity.Rare;

        protected override IEnumerable<DynamicVar> CanonicalVars => new List<DynamicVar> 
        { 
            new CardsVar(1) 
        };

        protected override IEnumerable<IHoverTip> ExtraHoverTips => new List<IHoverTip> 
        { 
            HoverTipFactory.FromKeyword(KoishiKeywords.Fumo) 
        };

        public override string PackedIconPath => $"res://mods/Komeiji_Koishi/images/relics/FumoCardRelic_Koishi_outline.png";
        protected override string PackedIconOutlinePath => $"res://mods/Komeiji_Koishi/images/relics/FumoCardRelic_Koishi.png";
        protected override string BigIconPath => $"res://mods/Komeiji_Koishi/images/relics/FumoCardRelic_Koishi.png";

  
        public override async Task BeforeHandDraw(Player player, PlayerChoiceContext choiceContext, CombatState combatState)
        {
   
            if (player == base.Owner && combatState.RoundNumber == 1)
            {
                base.Flash(); 
                
     
                for (int i = 0; i < base.DynamicVars.Cards.IntValue; i++)
                {
                    await FumoPool.CreateRandomFumoInHand(base.Owner, combatState);
                }
            }
        }
    }
}