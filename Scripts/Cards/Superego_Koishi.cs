using System.Collections.Generic;
using System.Threading.Tasks;
using BaseLib.Abstracts;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Localization.DynamicVars; 
using MegaCrit.Sts2.Core.HoverTips; 
using MegaCrit.Sts2.Core.Models.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using KomeijiKoishi.Pools;
using KomeijiKoishi.Powers;
using System;
using BaseLib.Utils;     
using MegaCrit.Sts2.Core.Models; 
using KomeijiKoishi.Utils_Koishi; 
using KomeijiKoishi.Enums;

namespace KomeijiKoishi.Cards
{
    [Pool(typeof(KoishiCardPool))]
    public sealed class Superego_Koishi : CustomCardModel
    {
        public Superego_Koishi() 
            : base(5, CardType.Power, CardRarity.Rare, TargetType.Self, true) { }

        public override string PortraitPath => $"res://mods/Komeiji_Koishi/images/cards/{GetType().Name}.png";

        public override IEnumerable<CardKeyword> CanonicalKeywords => new[] { CardKeyword.Ethereal ,KoishiKeywords.Closed , KoishiKeywords.Bloom };
        protected override IEnumerable<DynamicVar> CanonicalVars => new List<DynamicVar> { new EnergyVar(1) };
        protected override IEnumerable<IHoverTip> ExtraHoverTips => new[] 
        { 
            base.EnergyHoverTip 
        };

        protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
        {
            var player = base.Owner as MegaCrit.Sts2.Core.Entities.Players.Player;
            if (player == null) return;

            await CreatureCmd.TriggerAnim(player.Creature, "Buff", player.Character!.CastAnimDelay);
            await PowerCmd.Apply<SuperegoPower>(player.Creature, 1m, player.Creature, this, false);
        }

        protected override void OnUpgrade()
        {
            base.EnergyCost.UpgradeBy(-1); 
        }
    }
}