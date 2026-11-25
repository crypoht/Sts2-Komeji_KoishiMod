using BaseLib.Utils;
using KomeijiKoishi.Cards.Danmaku;
using KomeijiKoishi.Pools;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Nodes.CommonUi;

namespace KomeijiKoishi.Relics;

[Pool(typeof(KoishiSharedRelicPool))]
public sealed class DeepBlueRose_Koishi : KoishiCollaborationRelic
{
    private readonly HashSet<CardModel> _pendingPlayExhausts = new();

    protected override IEnumerable<IHoverTip> ExtraHoverTips =>
        [HoverTipFactory.FromKeyword(CardKeyword.Exhaust), HoverTipFactory.FromCard<ConsciousOverflow_Koishi>()];

    public override Task BeforeCombatStart()
    {
        _pendingPlayExhausts.Clear();
        return Task.CompletedTask;
    }

    public override Task AfterCardPlayed(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        var card = cardPlay.Card;
        if (card.Owner == Owner && card.Type == CardType.Skill && cardPlay.IsLastInSeries &&
            cardPlay.ResultPile == PileType.Exhaust && card.Pile?.Type == PileType.Play)
            _pendingPlayExhausts.Add(card);

        return Task.CompletedTask;
    }

    public override async Task AfterCardChangedPiles(CardModel card, PileType oldPileType, AbstractModel? clonedBy)
    {
        if (oldPileType != PileType.Play || !_pendingPlayExhausts.Remove(card) || card.Owner != Owner ||
            card.Pile?.Type != PileType.Exhaust || card.CombatState is not { } combat || CombatManager.Instance.IsOverOrEnding)
            return;

        Flash();
        var addResults = await CardPileCmd.AddGeneratedCardsToCombat(
            [card.CreateClone(), combat.CreateCard<ConsciousOverflow_Koishi>(Owner)],
            PileType.Discard, Owner, CardPilePosition.Bottom);
        CardCmd.PreviewCardPileAdd(addResults, 1.0f, CardPreviewStyle.HorizontalLayout);
    }
}
