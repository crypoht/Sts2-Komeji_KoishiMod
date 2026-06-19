using System.Collections.Generic;
using System.Threading.Tasks;
using BaseLib.Abstracts;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.ValueProps;
using KomeijiKoishi.Pools;
using System;
using BaseLib.Utils;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Models;


namespace KomeijiKoishi.Cards
{
    [Pool(typeof(KoishiCardPool))]
    public sealed class LookingGlassSelf_Koishi : CustomCardModel
    {
        public LookingGlassSelf_Koishi() 
            : base(0, CardType.Skill, CardRarity.Rare, TargetType.Self, true) 
        {
        }

        public override string PortraitPath => $"res://mods/Komeiji_Koishi/images/cards/{GetType().Name}.png";

        protected override IEnumerable<DynamicVar> CanonicalVars => new List<DynamicVar> 
        { 
            new BlockVar(1m, ValueProp.Unpowered) 
        };

        protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
        {
            var player = base.Owner as MegaCrit.Sts2.Core.Entities.Players.Player;
            if (player == null) return;

            var exhaustPile = PileType.Exhaust.GetPile(player);
            int count = (exhaustPile != null && exhaustPile.Cards != null) ? exhaustPile.Cards.Count : 0;
            {
                decimal totalBlock = count * base.DynamicVars.Block.BaseValue;
                await CreatureCmd.GainBlock(player.Creature, totalBlock, ValueProp.Move, cardPlay, false);
            }
        }

        protected override void OnUpgrade()
        {
            base.AddKeyword(CardKeyword.Retain);
        }
    }
}