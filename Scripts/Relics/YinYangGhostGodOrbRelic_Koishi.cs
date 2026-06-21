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
    public sealed class YinYangGhostGodOrbRelic_Koishi : CustomRelicModel
    {
        public override RelicRarity Rarity => RelicRarity.Ancient;

        protected override IEnumerable<DynamicVar> CanonicalVars => new List<DynamicVar>
        {
            new CardsVar(1)
        };

        protected override IEnumerable<IHoverTip> ExtraHoverTips =>
            HoverTipFactory.FromCardWithCardHoverTips<YinYangGhostGodOrb_Koishi>(true);

        public override string PackedIconPath => "res://mods/Komeiji_Koishi/images/relics/YinYangGhostGodOrbRelic_Koishi.png";
        protected override string PackedIconOutlinePath => "res://mods/Komeiji_Koishi/images/relics/YinYangGhostGodOrbRelic_Koishi.png";
        protected override string BigIconPath => "res://mods/Komeiji_Koishi/images/relics/YinYangGhostGodOrbRelic_Koishi.png";

        public override async Task AfterObtained()
        {
            CardModel card = base.Owner.RunState.CreateCard<YinYangGhostGodOrb_Koishi>(base.Owner);
            CardCmd.Upgrade(card, CardPreviewStyle.None);

            CardPileAddResult addResult = await CardPileCmd.Add(card, PileType.Deck, CardPilePosition.Bottom, null, false);
            CardCmd.PreviewCardPileAdd(addResult, 2f, CardPreviewStyle.HorizontalLayout);
        }
    }
}
