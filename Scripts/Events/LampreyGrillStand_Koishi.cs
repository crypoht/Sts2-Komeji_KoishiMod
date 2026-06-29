using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BaseLib.Abstracts;
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
    public sealed class LampreyGrillStand_Koishi : CustomEventModel
    {
        public override string? CustomInitialPortraitPath => "res://mods/Komeiji_Koishi/images/events/LampreyGrillStand_Koishi.png";

        protected override IEnumerable<DynamicVar> CanonicalVars => new DynamicVar[]
        {
            new HealVar(3m),
            new DynamicVar("SetMealCost", 75m),
            new DynamicVar("SetMealHeal", 10m),
            new DynamicVar("SetMealUpgrades", 1m),
            new DynamicVar("FestivalCost", 200m),
            new DynamicVar("FestivalHeal", 20m),
            new DynamicVar("FestivalUpgrades", 3m),
            new DamageVar(12m, ValueProp.Unblockable | ValueProp.Unpowered)
        };

        public override bool IsAllowed(IRunState runState)
        {
            return runState.CurrentActIndex == 1 && runState.Players.All(player => player.Gold > 100);
        }

        protected override IReadOnlyList<EventOption> GenerateInitialOptions()
        {
            Player owner = Owner!;
            EventOption setMealOption = owner.Gold >= DynamicVars["SetMealCost"].IntValue
                ? new EventOption(this, FavoriteSetMeal, $"{Id.Entry}.pages.INITIAL.options.FAVORITE_SET_MEAL", Array.Empty<IHoverTip>())
                : LockedOption("LOCKED", "INITIAL");

            EventOption festivalOption = owner.Gold >= DynamicVars["FestivalCost"].IntValue
                ? new EventOption(this, EdoBoatFestival, $"{Id.Entry}.pages.INITIAL.options.EDO_BOAT_FESTIVAL", Array.Empty<IHoverTip>())
                : LockedOption("LOCKED", "INITIAL");

            return new[]
            {
                new EventOption(this, RiceBallOnly, $"{Id.Entry}.pages.INITIAL.options.RICE_BALL_ONLY", Array.Empty<IHoverTip>()),
                setMealOption,
                festivalOption,
                new EventOption(this, EatTheOwner, $"{Id.Entry}.pages.INITIAL.options.EAT_THE_OWNER", HoverTipFactory.FromRelic<NightSparrowWings_Koishi>()).ThatDoesDamage(DynamicVars.Damage.BaseValue)
            };
        }

        private async Task RiceBallOnly()
        {
            await CreatureCmd.Heal(Owner!.Creature, DynamicVars.Heal.BaseValue, true);
            SetEventFinished(PageDescription("SATISFIED"));
        }

        private async Task FavoriteSetMeal()
        {
            Player owner = Owner!;
            await PlayerCmd.LoseGold(DynamicVars["SetMealCost"].BaseValue, owner, GoldLossType.Spent);
            await CreatureCmd.Heal(owner.Creature, DynamicVars["SetMealHeal"].BaseValue, true);
            UpgradeRandomCards(owner, DynamicVars["SetMealUpgrades"].IntValue);
            SetEventFinished(PageDescription("SATISFIED"));
        }

        private async Task EdoBoatFestival()
        {
            Player owner = Owner!;
            await PlayerCmd.LoseGold(DynamicVars["FestivalCost"].BaseValue, owner, GoldLossType.Spent);
            await CreatureCmd.Heal(owner.Creature, DynamicVars["FestivalHeal"].BaseValue, true);
            UpgradeRandomCards(owner, DynamicVars["FestivalUpgrades"].IntValue);
            SetEventFinished(PageDescription("SATISFIED"));
        }

        private async Task EatTheOwner()
        {
            Player owner = Owner!;
            await CreatureCmd.Damage(new ThrowingPlayerChoiceContext(), owner.Creature, DynamicVars.Damage, null, null);
            await RelicCmd.Obtain<NightSparrowWings_Koishi>(owner);
            SetEventFinished(PageDescription("WINGS"));
        }

        private void UpgradeRandomCards(Player owner, int count)
        {
            List<CardModel> cards = PileType.Deck.GetPile(owner).Cards
                .Where(card => card.IsUpgradable)
                .ToList()
                .StableShuffle(Rng)
                .Take(count)
                .ToList();

            if (cards.Count > 0)
            {
                CardCmd.Upgrade(cards, CardPreviewStyle.HorizontalLayout);
            }
        }
    }
}
