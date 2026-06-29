using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BaseLib.Abstracts;
using KomeijiKoishi.Cards;
using KomeijiKoishi.Enchantments;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Events;
using MegaCrit.Sts2.Core.Extensions;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Nodes;
using MegaCrit.Sts2.Core.Nodes.CommonUi;
using MegaCrit.Sts2.Core.Nodes.Vfx;
using MegaCrit.Sts2.Core.Runs;

namespace KomeijiKoishi.Events
{
    public sealed class MiracleShrineMaiden_Koishi : CustomEventModel
    {
        public override bool IsShared => true;

        public override string? CustomInitialPortraitPath => "res://mods/Komeiji_Koishi/images/events/MiracleShrineMaiden_Koishi.png";

        public override bool IsAllowed(IRunState runState)
        {
            return runState.CurrentActIndex == 1 || runState.CurrentActIndex == 2;
        }

        protected override IEnumerable<DynamicVar> CanonicalVars => new DynamicVar[]
        {
            new HealVar(12m),
            new CardsVar(2),
            new DynamicVar("Upgrades", 2m),
            new DynamicVar("Miracle", 1m)
        };

        protected override IReadOnlyList<EventOption> GenerateInitialOptions()
        {
            return new[]
            {
                new EventOption(this, Talk, $"{Id.Entry}.pages.INITIAL.options.TALK", Array.Empty<IHoverTip>()),
                new EventOption(this, Ignore, $"{Id.Entry}.pages.INITIAL.options.IGNORE", Array.Empty<IHoverTip>())
            };
        }

        private Task Talk()
        {
            SetEventState(PageDescription("MEETING"), new[]
            {
                new EventOption(this, AcceptBlessing, $"{Id.Entry}.pages.MEETING.options.ACCEPT_BLESSING", HoverTipFactory.FromEnchantment<MiracleEnchantment>(DynamicVars["Miracle"].IntValue)),
                new EventOption(this, AcceptOmamori, $"{Id.Entry}.pages.MEETING.options.ACCEPT_OMAMORI", Array.Empty<IHoverTip>()),
                new EventOption(this, WatchMoriyaDance, $"{Id.Entry}.pages.MEETING.options.WATCH_MORIYA_DANCE", HoverTipFactory.FromCardWithCardHoverTips<MoriyaDance_Koishi>(false))
            });

            return Task.CompletedTask;
        }

        private async Task Ignore()
        {
            Player owner = Owner!;
            await CreatureCmd.Heal(owner.Creature, DynamicVars.Heal.IntValue, true);
            SetEventFinished(PageDescription("IGNORE"));
        }

        private Task AcceptBlessing()
        {
            Player owner = Owner!;
            MiracleEnchantment miracle = ModelDb.Enchantment<MiracleEnchantment>();
            List<CardModel> cards = PileType.Deck.GetPile(owner).Cards
                .Where(miracle.CanEnchant)
                .ToList();
            if (cards.Count > 0)
            {
                CardModel card = cards.StableShuffle(Rng).First();
                MiracleEnchantment? applied = CardCmd.Enchant<MiracleEnchantment>(card, DynamicVars["Miracle"].BaseValue);
                if (applied != null)
                {
                    NCardEnchantVfx? enchantVfx = NCardEnchantVfx.Create(card);
                    if (enchantVfx != null)
                    {
                        NRun.Instance?.GlobalUi.CardPreviewContainer.AddChildSafely(enchantVfx);
                    }
                }
            }

            SetEventFinished(PageDescription("BLESSING"));
            return Task.CompletedTask;
        }

        private Task AcceptOmamori()
        {
            Player owner = Owner!;
            List<CardModel> cards = PileType.Deck.GetPile(owner).Cards
                .Where(card => card.IsUpgradable)
                .ToList()
                .StableShuffle(Rng)
                .Take(DynamicVars["Upgrades"].IntValue)
                .ToList();

            if (cards.Count > 0)
            {
                CardCmd.Upgrade(cards, CardPreviewStyle.HorizontalLayout);
            }

            SetEventFinished(PageDescription("OMAMORI"));
            return Task.CompletedTask;
        }

        private async Task WatchMoriyaDance()
        {
            Player owner = Owner!;
            List<CardModel> rareCards = owner.Character.CardPool
                .GetUnlockedCards(owner.UnlockState, owner.RunState.CardMultiplayerConstraint)
                .Where(card => card.Rarity == CardRarity.Rare)
                .ToList()
                .StableShuffle(Rng)
                .Take(DynamicVars.Cards.IntValue)
                .Select(card => owner.RunState.CreateCard(card, owner))
                .ToList();

            if (rareCards.Count > 0)
            {
                IReadOnlyList<CardPileAddResult> results = await CardPileCmd.Add(rareCards, PileType.Deck, CardPilePosition.Bottom, null, false);
                CardCmd.PreviewCardPileAdd(results, 2f, CardPreviewStyle.HorizontalLayout);
            }

            await CardPileCmd.AddCurseToDeck<MoriyaDance_Koishi>(owner);
            SetEventFinished(PageDescription("MORIYA_DANCE"));
        }
    }
}
