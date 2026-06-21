using System.Collections.Generic;
using System.Threading.Tasks;
using BaseLib.Abstracts;
using BaseLib.Utils;
using KomeijiKoishi.Cards;
using KomeijiKoishi.Pools;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Relics;
using MegaCrit.Sts2.Core.Nodes.CommonUi;

namespace KomeijiKoishi.Relics
{
    [Pool(typeof(KoishiSharedRelicPool))]
    public sealed class FrogCard_Koishi : CustomRelicModel
    {
        public override RelicRarity Rarity => RelicRarity.Ancient;

        protected override IEnumerable<DynamicVar> CanonicalVars => new List<DynamicVar>
        {
            new CardsVar(2)
        };

        protected override IEnumerable<IHoverTip> ExtraHoverTips =>
            HoverTipFactory.FromCardWithCardHoverTips<ExplosiveRedFrog_Koishi>(false);

        public override string PackedIconPath => $"res://mods/Komeiji_Koishi/images/relics/FrogCard_Koishi.png";
        protected override string PackedIconOutlinePath => $"res://mods/Komeiji_Koishi/images/relics/FrogCard_Koishi.png";
        protected override string BigIconPath => $"res://mods/Komeiji_Koishi/images/relics/FrogCard_Koishi.png";

        public override async Task AfterObtained()
        {
            List<CardPileAddResult> addResults = new List<CardPileAddResult>();
            for (int i = 0; i < base.DynamicVars.Cards.IntValue; i++)
            {
                CardModel frog = base.Owner.RunState.CreateCard<ExplosiveRedFrog_Koishi>(base.Owner);
                CardPileAddResult addResult = await CardPileCmd.Add(frog, PileType.Deck, CardPilePosition.Bottom, null, false);
                addResults.Add(addResult);
            }

            CardCmd.PreviewCardPileAdd(addResults, 2f, CardPreviewStyle.HorizontalLayout);
        }
    }
}
