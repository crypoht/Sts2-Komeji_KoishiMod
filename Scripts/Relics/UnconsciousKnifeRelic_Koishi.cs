using BaseLib.Utils;
using KomeijiKoishi.Cards;
using KomeijiKoishi.Pools;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Models;

namespace KomeijiKoishi.Relics;

[Pool(typeof(KoishiSharedRelicPool))]
public sealed class UnconsciousKnifeRelic_Koishi : KoishiCollaborationRelic
{
    public override bool HasUponPickupEffect => true;
    protected override IEnumerable<IHoverTip> ExtraHoverTips => [HoverTipFactory.FromCard<UnconsciousKnife_Koishi>()];

    public override async Task AfterObtained()
    {
        CardModel card = Owner.RunState.CreateCard<UnconsciousKnife_Koishi>(Owner);
        CardCmd.PreviewCardPileAdd(await CardPileCmd.Add(card, PileType.Deck, CardPilePosition.Bottom));
    }
}
