using System.Collections.Generic;
using System.Threading.Tasks;
using BaseLib.Abstracts;
using BaseLib.Utils;
using KomeijiKoishi.Pools;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Factories;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.CardPools;
using MegaCrit.Sts2.Core.Models.Cards;

namespace KomeijiKoishi.Cards.Fumo
{
    [Pool(typeof(TokenCardPool))]
    public sealed class PinkKoishiFumo_Koishi : CustomCardModel
    {
        public PinkKoishiFumo_Koishi()
            : base(0, CardType.Skill, CardRarity.Token, TargetType.Self, true)
        {
        }

        public override string PortraitPath => $"res://mods/Komeiji_Koishi/images/cards/fumo/{GetType().Name}.png";

        public override IEnumerable<CardKeyword> CanonicalKeywords => new[] { CardKeyword.Exhaust };

        protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
        {
            RelicModel relic = RelicFactory.PullNextRelicFromFront(base.Owner).ToMutable();
            await RelicCmd.Obtain(relic, base.Owner, -1);
        }

        protected override void OnUpgrade()
        {
            base.EnergyCost.UpgradeBy(+385);
        }
    }
}
