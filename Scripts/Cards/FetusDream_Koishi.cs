using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using BaseLib.Utils;     
using BaseLib.Abstracts; 
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer; 
using KomeijiKoishi.Pools; 
using KomeijiKoishi.Powers; 
using KomeijiKoishi.Utils_Koishi; 
using KomeijiKoishi.Enums;
using MegaCrit.Sts2.Core.HoverTips;

namespace KomeijiKoishi.Cards
{
    [Pool(typeof(KoishiCardPool))]
    public sealed class FetusDream_Koishi : CustomCardModel
    {
        public FetusDream_Koishi() : base(3, CardType.Power, CardRarity.Rare, TargetType.Self, true) { }

        public override string PortraitPath => $"res://mods/Komeiji_Koishi/images/cards/{GetType().Name}.png";

        protected override IEnumerable<IHoverTip> ExtraHoverTips => new[] 
        { 
            HoverTipFactory.FromKeyword(KoishiKeywords.Unconscious) 
        };
        protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
        {
            var owner = base.Owner;
            if (owner != null)
            {
                await PowerCmd.Apply<FetusDreamPower>(owner.Creature, 1m, owner.Creature, this, false);
            }
        }

        protected override void OnUpgrade() => base.EnergyCost.UpgradeBy(-1);
    }
}