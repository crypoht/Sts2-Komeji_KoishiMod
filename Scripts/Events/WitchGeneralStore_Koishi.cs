using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BaseLib.Abstracts;
using KomeijiKoishi.Cards;
using KomeijiKoishi.Relics;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Gold;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Events;
using MegaCrit.Sts2.Core.Extensions;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Nodes.CommonUi;
using MegaCrit.Sts2.Core.Runs;
using MegaCrit.Sts2.Core.ValueProps;

namespace KomeijiKoishi.Events
{
    public sealed class WitchGeneralStore_Koishi : CustomEventModel
    {
        private const int InitialWinChance = 40;
        private const int WinChanceIncrease = 10;
        private int chessAttempts;

        protected override IEnumerable<DynamicVar> CanonicalVars => new DynamicVar[]
        {
            new DynamicVar("WinChance", InitialWinChance),
            new DamageVar(15m, ValueProp.Unblockable | ValueProp.Unpowered),
            new GoldVar(150),
            new GoldVar("ArtworkGold", 250),
            new CardsVar(1)
        };

        public override string? CustomInitialPortraitPath =>
            "res://mods/Komeiji_Koishi/images/events/WitchGeneralStore_Koishi.png";

        public override bool IsAllowed(IRunState runState)
        {
            return runState.Players.All(player => player.Creature.CurrentHp > 20 || player.Gold > 150);
        }

        protected override IReadOnlyList<EventOption> GenerateInitialOptions()
        {
            Player owner = Owner!;
            EventOption buyKnifeOption = owner.Gold >= DynamicVars.Gold.IntValue
                ? new EventOption(this, BuyKnife, $"{Id.Entry}.pages.INITIAL.options.BUY_KNIFE", GetKnifeHoverTips(owner.RunState.CurrentActIndex))
                : new EventOption(this, null, $"{Id.Entry}.pages.INITIAL.options.LOCKED_GOLD", Array.Empty<IHoverTip>());

            EventOption buyArtworkOption = owner.Gold >= DynamicVars["ArtworkGold"].IntValue
                ? new EventOption(this, BuyArtwork, $"{Id.Entry}.pages.INITIAL.options.{GetArtworkOptionKey(owner.RunState.CurrentActIndex)}", GetArtworkHoverTips(owner.RunState.CurrentActIndex))
                : new EventOption(this, null, $"{Id.Entry}.pages.INITIAL.options.LOCKED_ARTWORK_GOLD", Array.Empty<IHoverTip>());

            EventOption chessOption = PileType.Deck.GetPile(owner).Cards.Count > 1
                ? new EventOption(this, PlayChess, $"{Id.Entry}.pages.INITIAL.options.PLAY_CHESS", Array.Empty<IHoverTip>())
                : new EventOption(this, null, $"{Id.Entry}.pages.INITIAL.options.LOCKED_CARDS", Array.Empty<IHoverTip>());

            return new[]
            {
                chessOption,
                new EventOption(this, DrinkMysteryBottle, $"{Id.Entry}.pages.INITIAL.options.DRINK", HoverTipFactory.FromRelic<MagicPotionBottle_Koishi>()).ThatDoesDamage(DynamicVars.Damage.BaseValue),
                buyKnifeOption,
                buyArtworkOption
            };
        }

        private async Task PlayChess()
        {
            if (PileType.Deck.GetPile(Owner!).Cards.Count <= 1)
            {
                ShowChessWinPage();
                return;
            }

            if (ShouldWinChess())
            {
                ShowChessWinPage();
                return;
            }

            await LoseRandomCard();
            chessAttempts++;
            DynamicVars["WinChance"].BaseValue = InitialWinChance + chessAttempts * WinChanceIncrease;
            SetEventState(PageDescription("CHESS_LOSE"), new[]
            {
                new EventOption(this, PlayChess, $"{Id.Entry}.pages.CHESS_LOSE.options.CONTINUE", Array.Empty<IHoverTip>())
            });
        }

        private bool ShouldWinChess()
        {
            Player owner = Owner!;
            if (PileType.Deck.GetPile(owner).Cards.Count <= 1)
            {
                return true;
            }

            if (!GetRemovableCards(owner).Any())
            {
                return true;
            }

            return Rng.NextFloat(1f) < DynamicVars["WinChance"].IntValue / 100f;
        }

        private async Task LoseRandomCard()
        {
            Player owner = Owner!;
            List<CardModel> removableCards = GetRemovableCards(owner).ToList();
            if (removableCards.Count == 0)
            {
                return;
            }

            CardModel? card = Rng.WeightedNextItem(removableCards, GetRemovalWeight);
            if (card == null)
            {
                return;
            }

            await CardPileCmd.RemoveFromDeck(card, true);
        }

        private IEnumerable<CardModel> GetRemovableCards(Player owner)
        {
            return PileType.Deck.GetPile(owner).Cards.Where(card => card.IsRemovable);
        }

        private static float GetRemovalWeight(CardModel? card)
        {
            if (card == null)
            {
                return 0f;
            }

            float weight = card.Rarity switch
            {
                CardRarity.Uncommon => 10f,
                CardRarity.Rare => 2f,
                CardRarity.Common => 4f,
                _ => 1f
            };

            if (card.IsUpgraded)
            {
                weight += 3f;
            }

            if (card.Enchantment != null)
            {
                weight += 2f;
            }

            return weight;
        }

        private void ShowChessWinPage()
        {
            string page = chessAttempts switch
            {
                0 => "CHESS_WIN_FIRST",
                >= 5 => "CHESS_WIN_MANY",
                _ => "CHESS_WIN_NORMAL"
            };

            SetEventState(PageDescription(page), new[]
            {
                new EventOption(this, TakeChessRewards, $"{Id.Entry}.pages.{page}.options.TAKE", HoverTipFactory.FromRelic<MagicPotionBottle_Koishi>().Concat(GetKnifeHoverTips(Owner!.RunState.CurrentActIndex)))
            });
        }

        private async Task TakeChessRewards()
        {
            Player owner = Owner!;
            await RelicCmd.Obtain<MagicPotionBottle_Koishi>(owner);
            await AddKnifeToDeck(owner);
            SetEventFinished(PageDescription("CHESS_DONE"));
        }

        private async Task DrinkMysteryBottle()
        {
            Player owner = Owner!;
            await CreatureCmd.Damage(new ThrowingPlayerChoiceContext(), owner.Creature, DynamicVars.Damage, null!, null!);
            await RelicCmd.Obtain<MagicPotionBottle_Koishi>(owner);
            SetEventFinished(PageDescription("DRINK_DONE"));
        }

        private async Task BuyKnife()
        {
            Player owner = Owner!;
            await PlayerCmd.LoseGold(DynamicVars.Gold.BaseValue, owner, GoldLossType.Spent);
            await AddKnifeToDeck(owner);
            SetEventFinished(PageDescription("KNIFE_DONE"));
        }

        private async Task BuyArtwork()
        {
            Player owner = Owner!;
            await PlayerCmd.LoseGold(DynamicVars["ArtworkGold"].BaseValue, owner, GoldLossType.Spent);
            await AddArtworkToDeck(owner);
            SetEventFinished(PageDescription("ARTWORK_DONE"));
        }

        private async Task AddKnifeToDeck(Player owner)
        {
            CardModel knife = owner.RunState.CreateCard(ModelDb.Card<KoishisKnife_Koishi>(), owner);
            int upgradeCount = GetKnifeUpgradeCount(owner.RunState.CurrentActIndex);
            for (int i = 0; i < upgradeCount; i++)
            {
                CardCmd.Upgrade(knife, CardPreviewStyle.None);
            }

            CardPileAddResult result = await CardPileCmd.Add(knife, PileType.Deck, CardPilePosition.Bottom, null, false);
            CardCmd.PreviewCardPileAdd(result, 2f, CardPreviewStyle.HorizontalLayout);
        }

        private static int GetKnifeUpgradeCount(int actIndex)
        {
            return actIndex switch
            {
                <= 0 => 0,
                1 => 8,
                _ => 12
            };
        }

        private async Task AddArtworkToDeck(Player owner)
        {
            CardModel artwork = owner.RunState.CreateCard(GetArtworkRewardModel(owner.RunState.CurrentActIndex), owner);
            CardCmd.Upgrade(artwork, CardPreviewStyle.None);

            CardPileAddResult result = await CardPileCmd.Add(artwork, PileType.Deck, CardPilePosition.Bottom, null, false);
            CardCmd.PreviewCardPileAdd(result, 2f, CardPreviewStyle.HorizontalLayout);
        }

        private static CardModel GetArtworkRewardModel(int actIndex)
        {
            return actIndex switch
            {
                <= 0 => ModelDb.Card<Artwork_Koishi>(),
                1 => ModelDb.Card<TrueArtwork_Koishi>(),
                _ => ModelDb.Card<TrueArtworkLiberated_Koishi>()
            };
        }

        private static IEnumerable<IHoverTip> GetArtworkHoverTips(int actIndex)
        {
            return actIndex switch
            {
                <= 0 => HoverTipFactory.FromCardWithCardHoverTips<Artwork_Koishi>(true),
                1 => HoverTipFactory.FromCardWithCardHoverTips<TrueArtwork_Koishi>(true),
                _ => HoverTipFactory.FromCardWithCardHoverTips<TrueArtworkLiberated_Koishi>(true)
            };
        }

        private static string GetArtworkOptionKey(int actIndex)
        {
            return actIndex switch
            {
                <= 0 => "BUY_ARTWORK",
                1 => "BUY_TRUE_ARTWORK",
                _ => "BUY_TRUE_ARTWORK_LIBERATED"
            };
        }

        private static IEnumerable<IHoverTip> GetKnifeHoverTips(int actIndex)
        {
            CardModel knife = (CardModel)ModelDb.Card<KoishisKnife_Koishi>().MutableClone();
            int upgradeCount = GetKnifeUpgradeCount(actIndex);
            for (int i = 0; i < upgradeCount; i++)
            {
                knife.UpgradeInternal();
                knife.FinalizeUpgradeInternal();
            }

            return new[] { HoverTipFactory.FromCard(knife, false) }.Concat(knife.HoverTips);
        }
    }
}
