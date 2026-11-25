using BaseLib.Utils;
using KomeijiKoishi.Pools;
using MegaCrit.Sts2.Core.CardSelection;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Models;

namespace KomeijiKoishi.Relics;

[Pool(typeof(KoishiSharedRelicPool))]
public sealed class FantasyFortuneSlip_Koishi : KoishiCollaborationRelic
{
    public override bool HasUponPickupEffect => true;

    public override async Task AfterObtained()
    {
        var cards = ModelDb.CardPool<KoishiCardPool>()
            .GetUnlockedCards(Owner.UnlockState, Owner.RunState.CardMultiplayerConstraint)
            .Where(card => card.Rarity is CardRarity.Common or CardRarity.Uncommon or CardRarity.Rare)
#if STS2_BETA
            .Where(card => card.IsAllowedInRun(Owner.RunState))
#endif
            .DistinctBy(card => card.Id)
            .OrderBy(card => card.Id.Entry, StringComparer.Ordinal)
            .Select(card => new CardCreationResult(Owner.RunState.CreateCard(card, Owner)))
            .ToList();
        if (cards.Count == 0)
            return;

        var prompt = new LocString("relics", Id.Entry + ".selectionScreenPrompt");
        var prefs = new CardSelectorPrefs(prompt, 0, cards.Count);
        var selected = await CardSelectCmd.FromSimpleGridForRewards(new BlockingPlayerChoiceContext(), cards, Owner, prefs);
        foreach (CardModel card in selected)
            CardCmd.PreviewCardPileAdd(await CardPileCmd.Add(card, PileType.Deck, CardPilePosition.Bottom));
    }
}
