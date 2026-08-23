using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text.Json;
using System.Threading.Tasks;
using BaseLib.Abstracts;
using BaseLib.Utils;
using KomeijiKoishi.Dataminer;
using KomeijiKoishi.Patches;
using KomeijiKoishi.Pools;
using MegaCrit.Sts2.Core.CardSelection;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Gold;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Potions;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.Factories;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.CardPools;
using MegaCrit.Sts2.Core.Models.Cards;
using MegaCrit.Sts2.Core.Models.Orbs;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.Nodes.CommonUi;
using MegaCrit.Sts2.Core.Saves.Runs;
using MegaCrit.Sts2.Core.ValueProps;

namespace KomeijiKoishi.Cards
{
    [Pool(typeof(TokenCardPool))]
    public class DataminerCard_Koishi : CustomCardModel
    {
        private DataminerEffect effect;
        private bool effectLoaded;

        [SavedProperty]
        public string SerializedEffect { get; private set; } = string.Empty;

        public DataminerCard_Koishi()
            : base(0, CardType.Skill, CardRarity.Token, TargetType.Self, false)
        {
        }

        // The portrait is replaced at runtime by DataminerCompositePortraitPatch.
        // Keep a real fallback asset so NCard does not repeatedly log a missing-resource error.
        public override string PortraitPath =>
            "res://mods/Komeiji_Koishi/images/error/DataminerCard_Koishi_1.png";

        public virtual int PortraitVariantIndex => 0;

        public override CardPoolModel VisualCardPool => ModelDb.CardPool<KoishiCardPool>();

        protected override IEnumerable<DynamicVar> CanonicalVars => new[]
        {
            new DynamicVar("Amount", 1m)
        };

        public void SetEffect(DataminerEffect value)
        {
            effect = value;
            effectLoaded = true;
            SerializedEffect = JsonSerializer.Serialize(value);
            EnergyCost.SetThisCombat(value.Cost, false);
        }

        private void EnsureEffectLoaded()
        {
            if (effectLoaded || SerializedEffect.Length == 0)
            {
                return;
            }

            try
            {
                effect = JsonSerializer.Deserialize<DataminerEffect>(SerializedEffect);
                effectLoaded = true;
                EnergyCost.SetThisCombat(effect.Cost, false);
            }
            catch (JsonException)
            {
                // Keep the existing effect for legacy cards with no compatible payload.
                effectLoaded = true;
            }
        }

        // The effect is already synchronized as part of the card transformation, so it
        // also provides a stable seed for purely visual randomization.
        public string CompositePortraitSeed
        {
            get
            {
                EnsureEffectLoaded();
                return $"{effect.CardType}|{effect.Cost}|{effect.Primary.Kind}|{effect.Amount}|"
                    + $"{FormatGeneratedCard(effect.Primary)}|"
                    + $"{effect.PlayCondition.Kind}:{effect.PlayCondition.Amount}|"
                    + $"{effect.ExtraCondition.Kind}:{effect.ExtraCondition.Amount}|"
                    + $"{effect.ExtraEffect.Kind}:{effect.ExtraEffect.Amount}:{effect.ExtraEffect.PowerId}:{effect.ExtraEffect.PileScope}:{effect.ExtraEffect.SecondaryAmount}|"
                    + $"{FormatGeneratedCard(effect.ExtraEffect)}|"
                    + $"{effect.PowerId}|{effect.ReturnPile}|{effect.ExtraPrimaryRepeats}";
            }
        }

        public bool AutoPlayAtTurnEnd
        {
            get
            {
                EnsureEffectLoaded();
                return effect.AutoPlayAtTurnEnd;
            }
        }

        public override CardType Type
        {
            get
            {
                EnsureEffectLoaded();
                return effect.CardType;
            }
        }

        public override TargetType TargetType
        {
            get
            {
                EnsureEffectLoaded();
                return effect.TargetType;
            }
        }

        protected override bool IsPlayable
        {
            get
            {
                EnsureEffectLoaded();
                return base.IsPlayable && (base.Owner == null || effect.PlayCondition.IsMet(base.Owner));
            }
        }

        protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
        {
            EnsureEffectLoaded();
            Player? player = base.Owner as Player;
            if (player == null || base.CombatState == null)
            {
                return;
            }

            if (!effect.PlayCondition.IsMet(player))
            {
                return;
            }

            await ApplySubEffect(choiceContext, player, cardPlay, effect.Primary);
            if (effect.ExtraEffect.Kind != DataminerEffectKind.None
                && effect.ExtraCondition.IsMet(player))
            {
                await ApplySubEffect(choiceContext, player, cardPlay, effect.ExtraEffect);
                for (int i = 0; i < effect.ExtraPrimaryRepeats; i++)
                {
                    await ApplySubEffect(choiceContext, player, cardPlay, effect.Primary);
                }
            }
        }

        private async Task ApplySubEffect(
            PlayerChoiceContext choiceContext,
            Player player,
            CardPlay cardPlay,
            DataminerSubEffect subEffect)
        {
            switch (subEffect.Kind)
            {
                case DataminerEffectKind.Damage:
                    if (cardPlay.Target != null)
                    {
                        await DamageCmd.Attack(subEffect.Amount).FromCard(this, cardPlay).Targeting(cardPlay.Target).Execute(choiceContext);
                    }
                    break;
                case DataminerEffectKind.DamageAll:
                    await DamageCmd.Attack(subEffect.Amount).FromCard(this, cardPlay).TargetingAllOpponents(base.CombatState!).Execute(choiceContext);
                    break;
                case DataminerEffectKind.Block:
                    await CreatureCmd.GainBlock(player.Creature, subEffect.Amount, ValueProp.Move, cardPlay, false);
                    break;
                case DataminerEffectKind.AllyBlock:
                    await CreatureCmd.GainBlock(cardPlay.Target ?? player.Creature, subEffect.Amount, ValueProp.Move, cardPlay, false);
                    break;
                case DataminerEffectKind.Draw:
                    await CardPileCmd.Draw(choiceContext, subEffect.Amount, player, false);
                    break;
                case DataminerEffectKind.Energy:
                    if (subEffect.Amount >= 0)
                    {
                        await PlayerCmd.GainEnergy(subEffect.Amount, player);
                    }
                    else
                    {
                        await PlayerCmd.LoseEnergy(-subEffect.Amount, player);
                    }
                    break;
                case DataminerEffectKind.Health:
                    if (subEffect.Amount >= 0)
                    {
                        await CreatureCmd.Heal(player.Creature, subEffect.Amount, true);
                    }
                    else
                    {
                        await CreatureCmd.Damage(choiceContext, player.Creature, -subEffect.Amount,
                            ValueProp.Unblockable | ValueProp.Unpowered, null, null);
                    }
                    break;
                case DataminerEffectKind.ExhaustRandomHand:
                    await ExhaustRandomHand(choiceContext, player, subEffect.Amount);
                    break;
                case DataminerEffectKind.EnemyBlock:
                    if (cardPlay.Target != null)
                    {
                        await CreatureCmd.GainBlock(cardPlay.Target, subEffect.Amount, ValueProp.Move, cardPlay, false);
                    }
                    break;
                case DataminerEffectKind.RandomPower:
                    PowerModel? power = DataminerPowerPool.Resolve(subEffect.PowerId);
                    if (power != null)
                    {
                        await PowerCmd.Apply(choiceContext, power.ToMutable(), player.Creature, subEffect.Amount, player.Creature, this, false);
                    }
                    break;
                case DataminerEffectKind.RandomBuff:
                    PowerModel? buff = DataminerBuffPool.Resolve(subEffect.PowerId);
                    if (buff != null)
                    {
                        await PowerCmd.Apply(choiceContext, buff.ToMutable(), player.Creature, subEffect.Amount, player.Creature, this, false);
                    }
                    break;
                case DataminerEffectKind.GenerateCards:
                    await GenerateCards(player, subEffect);
                    break;
                case DataminerEffectKind.Gold:
                    if (subEffect.Amount >= 0)
                    {
                        await PlayerCmd.GainGold(subEffect.Amount, player);
                    }
                    else
                    {
                        await PlayerCmd.LoseGold(-subEffect.Amount, player, GoldLossType.Lost);
                    }
                    break;
                case DataminerEffectKind.Potions:
                    await GeneratePotions(player, subEffect.Amount);
                    break;
                case DataminerEffectKind.DamagePerPile:
                    if (cardPlay.Target != null)
                    {
                        int count = GetCards(player, subEffect.PileScope).Count;
                        await DamageCmd.Attack(subEffect.Amount * count).FromCard(this, cardPlay).Targeting(cardPlay.Target).Execute(choiceContext);
                    }
                    break;
                case DataminerEffectKind.ChannelOrb:
                    await ChannelOrbs(choiceContext, player, subEffect);
                    break;
                case DataminerEffectKind.Combo:
                    TryAddCombo(player, subEffect.Amount);
                    break;
                case DataminerEffectKind.DiscardAllHand:
                    await CardCmd.Discard(choiceContext, PileType.Hand.GetPile(player).Cards.ToList());
                    break;
                case DataminerEffectKind.ExhaustAllHand:
                    foreach (CardModel handCard in PileType.Hand.GetPile(player).Cards.ToList())
                    {
                        await CardCmd.Exhaust(choiceContext, handCard, false, false);
                    }
                    break;
                case DataminerEffectKind.ExhaustSelectedHand:
                    await ExhaustSelectedHand(choiceContext, player, subEffect.Amount);
                    break;
                case DataminerEffectKind.AutoPlayPile:
                    await AutoPlayPile(choiceContext, player, cardPlay, subEffect);
                    break;
                case DataminerEffectKind.Poison:
                    if (cardPlay.Target != null)
                    {
                        await PowerCmd.Apply<PoisonPower>(choiceContext, cardPlay.Target, subEffect.Amount, player.Creature, this, false);
                    }
                    break;
                case DataminerEffectKind.UpgradePile:
                    UpgradeCards(GetCards(player, subEffect.PileScope), subEffect.Amount);
                    break;
                case DataminerEffectKind.UpgradeAll:
                    UpgradeCards(GetCards(player, DataminerPileScope.All), int.MaxValue);
                    break;
                case DataminerEffectKind.Summon:
                    await PowerCmd.Apply<SummonNextTurnPower>(choiceContext, player.Creature, subEffect.Amount, player.Creature, this, false);
                    break;
                case DataminerEffectKind.Stars:
                    await PlayerCmd.GainStars(subEffect.Amount, player);
                    break;
                case DataminerEffectKind.EnchantPile:
                    EnchantCards(GetCards(player, subEffect.PileScope), subEffect.PowerId, subEffect.SecondaryAmount, subEffect.Amount);
                    break;
                case DataminerEffectKind.RandomizeHandCost:
                    RandomizeHandCost(player, subEffect.Amount);
                    break;
            }
        }

        private static List<CardModel> GetCards(Player player, DataminerPileScope? scope)
        {
            IEnumerable<PileType> piles = scope switch
            {
                DataminerPileScope.Deck => new[] { PileType.Deck },
                DataminerPileScope.Exhaust => new[] { PileType.Exhaust },
                DataminerPileScope.Draw => new[] { PileType.Draw },
                DataminerPileScope.Discard => new[] { PileType.Discard },
                DataminerPileScope.Hand => new[] { PileType.Hand },
                _ => new[] { PileType.Deck, PileType.Exhaust, PileType.Draw, PileType.Discard, PileType.Hand }
            };
            return piles.SelectMany(pile => pile.GetPile(player).Cards).ToList();
        }

        private static async Task ChannelOrbs(PlayerChoiceContext context, Player player, DataminerSubEffect effect)
        {
            if (!Enum.TryParse(effect.PowerId, out DataminerOrbKind orbKind))
            {
                return;
            }

            for (int i = 0; i < effect.Amount; i++)
            {
                switch (orbKind)
                {
                    case DataminerOrbKind.Lightning:
                        await OrbCmd.Channel<LightningOrb>(context, player);
                        break;
                    case DataminerOrbKind.Glass:
                        await OrbCmd.Channel<GlassOrb>(context, player);
                        break;
                    case DataminerOrbKind.Dark:
                        await OrbCmd.Channel<DarkOrb>(context, player);
                        break;
                    case DataminerOrbKind.Frost:
                        await OrbCmd.Channel<FrostOrb>(context, player);
                        break;
                }
            }
        }

        private async Task ExhaustSelectedHand(PlayerChoiceContext context, Player player, int amount)
        {
            if (amount <= 0 || PileType.Hand.GetPile(player).Cards.Count == 0)
            {
                return;
            }

            CardSelectorPrefs prefs = new CardSelectorPrefs(CardSelectorPrefs.EnchantSelectionPrompt, 0, amount);
            IEnumerable<CardModel> selected = await CardSelectCmd.FromHand(context, player, prefs, null, this);
            foreach (CardModel card in selected.ToList())
            {
                await CardCmd.Exhaust(context, card, false, false);
            }
        }

        private static async Task AutoPlayPile(PlayerChoiceContext context, Player player, CardPlay cardPlay, DataminerSubEffect effect)
        {
            if (effect.Amount <= 0)
            {
                return;
            }

            List<CardModel> cards = GetCards(player, effect.PileScope)
                .Where(card => card is not DataminerCard_Koishi && card.Type != CardType.Status && card.Type != CardType.Curse)
                .Take(effect.Amount)
                .ToList();
            foreach (CardModel card in cards)
            {
                await CardPileCmd.Add(card, PileType.Play, CardPilePosition.Bottom, null, false);
                await CardCmd.AutoPlay(context, card, cardPlay.Target, AutoPlayType.Default, false, false);
            }
        }

        private static void UpgradeCards(List<CardModel> cards, int amount)
        {
            List<CardModel> selected = cards.Where(card => card.IsUpgradable).Take(amount).ToList();
            if (selected.Count > 0)
            {
                CardCmd.Upgrade(selected, CardPreviewStyle.HorizontalLayout);
            }
        }

        private static void EnchantCards(List<CardModel> cards, string? enchantmentId, int enchantmentAmount, int count)
        {
            EnchantmentModel? canonical = DataminerEnchantmentPool.Resolve(enchantmentId);
            if (canonical == null || count <= 0)
            {
                return;
            }

            foreach (CardModel card in cards.Take(count))
            {
                EnchantmentModel enchantment = canonical.CanonicalInstance.ToMutable();
                if (enchantment.CanEnchant(card))
                {
                    CardCmd.Enchant(enchantment, card, enchantmentAmount);
                }
            }
        }

        private static void RandomizeHandCost(Player player, int cost)
        {
            foreach (CardModel card in PileType.Hand.GetPile(player).Cards.ToList())
            {
                if (!card.EnergyCost.CostsX)
                {
                    card.EnergyCost.SetThisCombat(cost, false);
                }
            }
        }

        private static void TryAddCombo(Player player, int amount)
        {
            foreach (object target in new object?[] { player.PlayerCombatState, player }.Where(value => value != null)!)
            {
                MethodInfo? method = target.GetType().GetMethods(BindingFlags.Instance | BindingFlags.Public)
                    .FirstOrDefault(candidate => candidate.Name.Contains("Combo", StringComparison.OrdinalIgnoreCase)
                        && candidate.GetParameters() is { Length: 1 } parameters
                        && parameters[0].ParameterType == typeof(int));
                if (method == null)
                {
                    continue;
                }

                method.Invoke(target, new object[] { amount });
                return;
            }
        }

        private async Task GenerateCards(Player player, DataminerSubEffect subEffect)
        {
            if (subEffect.Amount <= 0 || subEffect.GeneratedCardDestination == null)
            {
                return;
            }

            CardModel? canonicalCard = ResolveGeneratedCard(subEffect);
            if (canonicalCard == null)
            {
                return;
            }

            for (int i = 0; i < subEffect.Amount; i++)
            {
                CardModel generatedCard;
                if (subEffect.GeneratedCardKind == DataminerGeneratedCardKind.SpecificDataminer)
                {
                    generatedCard = CreateDataminerCard(
                        player,
                        subEffect.GeneratedCardDestination != DataminerGeneratedCardDestination.Deck);
                }
                else
                {
                    generatedCard = player.RunState.CreateCard(canonicalCard, player);
                }

                if (subEffect.GeneratedCardDestination == DataminerGeneratedCardDestination.Deck)
                {
                    using (DataminerDeckProtectionPatch.AllowInternalGeneration())
                    {
                        await CardPileCmd.Add(generatedCard, PileType.Deck, CardPilePosition.Bottom, null, false);
                    }
                    continue;
                }

                PileType pileType = subEffect.GeneratedCardDestination switch
                {
                    DataminerGeneratedCardDestination.Hand => PileType.Hand,
                    DataminerGeneratedCardDestination.Draw => PileType.Draw,
                    DataminerGeneratedCardDestination.Discard => PileType.Discard,
                    DataminerGeneratedCardDestination.Exhaust => PileType.Exhaust,
                    _ => PileType.Hand
                };

                await CardPileCmd.AddGeneratedCardToCombat(
                    generatedCard,
                    pileType,
                    player,
                    CardPilePosition.Bottom);
            }
        }

        private CardModel? ResolveGeneratedCard(DataminerSubEffect subEffect)
        {
            if (subEffect.GeneratedCardKind == DataminerGeneratedCardKind.SpecificDataminer)
            {
                return ModelDb.Card<DataminerCard_Koishi>();
            }

            if (string.IsNullOrEmpty(subEffect.GeneratedCardId))
            {
                return null;
            }

            return ModelDb.AllCards.FirstOrDefault(card => card.Id.Entry == subEffect.GeneratedCardId);
        }

        private DataminerCard_Koishi CreateDataminerCard(Player player, bool combatScoped)
        {
            int variant = player.RunState.Rng.CombatCardGeneration.NextInt(7);
            DataminerCard_Koishi card = variant switch
            {
                0 => combatScoped
                    ? ((CombatState)player.Creature.CombatState!).CreateCard<DataminerCard_Koishi>(player)
                    : player.RunState.CreateCard<DataminerCard_Koishi>(player),
                1 => combatScoped
                    ? ((CombatState)player.Creature.CombatState!).CreateCard<DataminerCard_Koishi_2>(player)
                    : player.RunState.CreateCard<DataminerCard_Koishi_2>(player),
                2 => combatScoped
                    ? ((CombatState)player.Creature.CombatState!).CreateCard<DataminerCard_Koishi_3>(player)
                    : player.RunState.CreateCard<DataminerCard_Koishi_3>(player),
                3 => combatScoped
                    ? ((CombatState)player.Creature.CombatState!).CreateCard<DataminerCard_Koishi_4>(player)
                    : player.RunState.CreateCard<DataminerCard_Koishi_4>(player),
                4 => combatScoped
                    ? ((CombatState)player.Creature.CombatState!).CreateCard<DataminerCard_Koishi_5>(player)
                    : player.RunState.CreateCard<DataminerCard_Koishi_5>(player),
                5 => combatScoped
                    ? ((CombatState)player.Creature.CombatState!).CreateCard<DataminerCard_Koishi_6>(player)
                    : player.RunState.CreateCard<DataminerCard_Koishi_6>(player),
                _ => combatScoped
                    ? ((CombatState)player.Creature.CombatState!).CreateCard<DataminerCard_Koishi_7>(player)
                    : player.RunState.CreateCard<DataminerCard_Koishi_7>(player)
            };
            card.SetEffect(DataminerEffect.Create(player));
            return card;
        }

        private static async Task GeneratePotions(Player player, int count)
        {
            if (count <= 0)
            {
                return;
            }

            IEnumerable<PotionModel> potions = PotionFactory.CreateRandomPotionsOutOfCombat(
                player,
                count,
                player.RunState.Rng.CombatPotionGeneration,
                null);

            foreach (PotionModel potion in potions)
            {
                await PotionCmd.TryToProcure(potion.ToMutable(), player, -1);
            }
        }

        private static string FormatGeneratedCard(DataminerSubEffect subEffect)
        {
            return subEffect.Kind == DataminerEffectKind.GenerateCards
                ? $"{subEffect.GeneratedCardKind}:{subEffect.GeneratedCardId}:{subEffect.GeneratedCardDestination}"
                : string.Empty;
        }

        private static async Task ExhaustRandomHand(PlayerChoiceContext choiceContext, Player player, int count)
        {
            List<CardModel> hand = new List<CardModel>(PileType.Hand.GetPile(player).Cards);
            for (int i = 0; i < count && hand.Count > 0; i++)
            {
                CardModel? card = player.RunState.Rng.CombatCardSelection.NextItem(hand);
                if (card == null)
                {
                    continue;
                }

                hand.Remove(card);
                await CardCmd.Exhaust(choiceContext, card, false, false);
            }
        }

        private static PileType GetStablePile(DataminerReturnPile returnPile)
        {
            return returnPile switch
            {
                DataminerReturnPile.DrawTop => PileType.Draw,
                DataminerReturnPile.DrawBottom => PileType.Draw,
                DataminerReturnPile.Hand => PileType.Hand,
                DataminerReturnPile.Exhaust => PileType.Exhaust,
                _ => PileType.Discard
            };
        }

#if STS2_BETA
        protected override CardLocation GetResultLocationForCardPlay()
        {
            CardLocation location = base.GetResultLocationForCardPlay();
            location.pileType = GetStablePile(effect.ReturnPile);
            location.position = effect.ReturnPile switch
            {
                DataminerReturnPile.DrawTop => CardPilePosition.Top,
                DataminerReturnPile.DrawBottom => CardPilePosition.Bottom,
                _ => CardPilePosition.Bottom
            };
            return location;
        }
#else
        protected override PileType GetResultPileTypeForCardPlay()
        {
            return GetStablePile(effect.ReturnPile);
        }
#endif
    }

    [Pool(typeof(TokenCardPool))]
    public sealed class DataminerCard_Koishi_2 : DataminerCard_Koishi
    {
        public override string PortraitPath =>
            "res://mods/Komeiji_Koishi/images/error/DataminerCard_Koishi_2.png";

        public override int PortraitVariantIndex => 1;
    }

    [Pool(typeof(TokenCardPool))]
    public sealed class DataminerCard_Koishi_3 : DataminerCard_Koishi
    {
        public override string PortraitPath =>
            "res://mods/Komeiji_Koishi/images/error/DataminerCard_Koishi_3.png";

        public override int PortraitVariantIndex => 2;
    }

    [Pool(typeof(TokenCardPool))]
    public sealed class DataminerCard_Koishi_4 : DataminerCard_Koishi
    {
        public override string PortraitPath =>
            "res://mods/Komeiji_Koishi/images/error/DataminerCard_Koishi_4.png";

        public override int PortraitVariantIndex => 3;
    }

    [Pool(typeof(TokenCardPool))]
    public sealed class DataminerCard_Koishi_5 : DataminerCard_Koishi
    {
        public override string PortraitPath =>
            "res://mods/Komeiji_Koishi/images/error/DataminerCard_Koishi_5.png";

        public override int PortraitVariantIndex => 4;
    }

    [Pool(typeof(TokenCardPool))]
    public sealed class DataminerCard_Koishi_6 : DataminerCard_Koishi
    {
        public override string PortraitPath =>
            "res://mods/Komeiji_Koishi/images/error/DataminerCard_Koishi_6.png";

        public override int PortraitVariantIndex => 5;
    }

    [Pool(typeof(TokenCardPool))]
    public sealed class DataminerCard_Koishi_7 : DataminerCard_Koishi
    {
        public override string PortraitPath =>
            "res://mods/Komeiji_Koishi/images/error/DataminerCard_Koishi_7.png";

        public override int PortraitVariantIndex => 6;
    }
}
