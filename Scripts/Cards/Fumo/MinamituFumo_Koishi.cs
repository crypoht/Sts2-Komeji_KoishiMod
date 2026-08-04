using System.Collections.Generic;
using System.Threading.Tasks;
using BaseLib.Abstracts;
using BaseLib.Utils;
using KomeijiKoishi.Pools;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.CardPools;
using MegaCrit.Sts2.Core.Models.Cards;
using MegaCrit.Sts2.Core.Models.Powers;

namespace KomeijiKoishi.Cards.Fumo
{
    [Pool(typeof(TokenCardPool))]
    public sealed class MinamituFumo_Koishi : CustomCardModel
    {
        public MinamituFumo_Koishi()
            : base(0, CardType.Skill, CardRarity.Token, TargetType.Self, true)
        {
        }

        public override string PortraitPath => $"res://mods/Komeiji_Koishi/images/cards/fumo/{GetType().Name}.png";

        public override IEnumerable<CardKeyword> CanonicalKeywords => new[] { CardKeyword.Exhaust };

        protected override IEnumerable<DynamicVar> CanonicalVars => new List<DynamicVar>
        {
            new PowerVar<HexPower>(1m)
        };

        public override async Task AfterCardChangedPiles(CardModel card, PileType oldPileType, AbstractModel? clonedBy)
        {
            if (card != this || base.Pile == null || base.Pile.Type != PileType.Hand)
            {
                return;
            }

            await ApplyHex(new ThrowingPlayerChoiceContext());
        }

        private async Task ApplyHex(PlayerChoiceContext choiceContext)
        {
            await PowerCmd.Apply<HexPower>(
                choiceContext,
                base.Owner.Creature,
                base.DynamicVars["HexPower"].BaseValue,
                base.Owner.Creature,
                this,
                false
            );
        }

        protected override void OnUpgrade()
        {
            base.EnergyCost.UpgradeBy(+385);
        }
    }
}
