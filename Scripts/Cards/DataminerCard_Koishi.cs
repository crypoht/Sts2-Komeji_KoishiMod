using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text.Json;
using System.Threading.Tasks;
using BaseLib.Abstracts;
using BaseLib.Utils;
using Godot;
using KomeijiKoishi.Dataminer;
using KomeijiKoishi.Patches;
using KomeijiKoishi.Pools;
using KomeijiKoishi.Powers;
using KomeijiKoishi.Relics;
using KomeijiKoishi.Multiplayer;
using KomeijiKoishi.Vfx;
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
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.CardPools;
using MegaCrit.Sts2.Core.Models.Cards;
using MegaCrit.Sts2.Core.Models.Orbs;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.Nodes.CommonUi;
using MegaCrit.Sts2.Core.Nodes.Vfx;
using MegaCrit.Sts2.Core.Rewards;
using MegaCrit.Sts2.Core.Rooms;
using MegaCrit.Sts2.Core.Saves.Runs;
using MegaCrit.Sts2.Core.Runs;
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

        public DataminerEffect GetEffectForDescription()
        {
            EnsureEffectLoaded();
            return effect;
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
                    + $"{effect.ExtraEffect.Kind}:{effect.ExtraEffect.Amount}:{effect.ExtraEffect.PowerId}:{effect.ExtraEffect.PileScope}:{effect.ExtraEffect.SecondaryAmount}:{effect.ExtraEffect.PowerTarget}|"
                    + $"{FormatGeneratedCard(effect.ExtraEffect)}|"
                    + $"{effect.PowerId}|{effect.ReturnPile}|{effect.ExtraPrimaryRepeats}|{effect.ComboAmount}|"
                    + $"{effect.Unplayable}|{effect.MustPlayFirst}|{effect.AbilityTrigger}|"
                    + $"{effect.AbilityEffect.Kind}:{effect.AbilityEffect.Amount}:{effect.AbilityEffect.PowerId}";
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

        public override IEnumerable<CardKeyword> CanonicalKeywords
        {
            get
            {
                EnsureEffectLoaded();
                return effect.Unplayable
                    ? new[] { CardKeyword.Unplayable }
                    : Array.Empty<CardKeyword>();
            }
        }

        public override bool ShouldPlay(CardModel card, AutoPlayType autoPlayType)
        {
            EnsureEffectLoaded();
            if (!effect.MustPlayFirst || base.Owner == null || card.Owner != base.Owner)
            {
                return true;
            }

            CardPile? pile = base.Pile;
            return pile == null
                || pile.Type != PileType.Hand
                || card == this
                || autoPlayType != AutoPlayType.None;
        }

        protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
        {
            EnsureEffectLoaded();
            Player? player = base.Owner as Player;
            if (player == null || base.CombatState == null)
            {
                return;
            }

            if (effect.CardType == CardType.Power)
            {
                await ApplyGeneratedAbility(choiceContext, player);
                return;
            }

            if (!effect.PlayCondition.IsMet(player))
            {
                return;
            }

            await ApplySubEffect(choiceContext, player, cardPlay, effect.Primary);
            if (effect.ComboAmount > 0)
            {
                TryAddCombo(player, effect.ComboAmount);
            }

            if (effect.ExtraEffect.Kind != DataminerEffectKind.None
                && effect.ExtraCondition.IsMet(player))
            {
                if (effect.ExtraEffect.Kind == DataminerEffectKind.RepeatPrimary)
                {
                    for (int i = 0; i < effect.ExtraEffect.Amount; i++)
                    {
                        await ApplySubEffect(choiceContext, player, cardPlay, effect.Primary);
                    }
                }
                else
                {
                    await ApplySubEffect(choiceContext, player, cardPlay, effect.ExtraEffect);
                }
            }
        }

        private async Task ApplyGeneratedAbility(PlayerChoiceContext context, Player player)
        {
            DataminerAbilityPower power = (DataminerAbilityPower)ModelDb.Power<DataminerAbilityPower>().ToMutable();
            power.Configure(effect.AbilityTrigger, effect.AbilityEffect);
            await PowerCmd.Apply(context, power, player.Creature, 1m, player.Creature, this, false);
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
                case DataminerEffectKind.HealthLossByPileCount:
                    await LoseHealthByPileCount(choiceContext, player);
                    break;
                case DataminerEffectKind.GenerateShivs:
                    await Shiv.CreateInHand(player, subEffect.Amount, base.CombatState!);
                    break;
                case DataminerEffectKind.GenerateSouls:
                    await AddSoulsToPile(player, PileType.Hand, subEffect.Amount);
                    break;
                case DataminerEffectKind.GenerateSoulsToPile:
                    await AddSoulsToPile(player, GetPileType(subEffect.PileScope), subEffect.Amount);
                    break;
                case DataminerEffectKind.RetrieveDiscardCards:
                    await RetrieveCards(choiceContext, player, PileType.Discard, subEffect.Amount, subEffect.PileScope);
                    break;
                case DataminerEffectKind.RetrieveDrawCards:
                    await RetrieveCards(choiceContext, player, PileType.Draw, subEffect.Amount, subEffect.PileScope);
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
                case DataminerEffectKind.SpecificPower:
                    PowerModel? power = DataminerPowerPool.Resolve(subEffect.PowerId);
                    if (power != null)
                    {
                        await ApplyPowerToTargets(choiceContext, player, power, subEffect);
                    }
                    break;
                case DataminerEffectKind.RandomBuff:
                    PowerModel? buff = DataminerBuffPool.Resolve(subEffect.PowerId);
                    if (buff != null)
                    {
                        await ApplyPowerToTargets(choiceContext, player, buff, subEffect);
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
                    await SummonRandomMonsters(player, subEffect);
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
                case DataminerEffectKind.DirectWin:
                    await WinCombat(player);
                    break;
                case DataminerEffectKind.MaxHp:
                    await ChangeMaxHp(choiceContext, player, subEffect);
                    break;
                case DataminerEffectKind.ExtraCardRewards:
                    AddExtraCardRewards(player, subEffect.Amount);
                    break;
                case DataminerEffectKind.ObtainRelic:
                    await ObtainRandomRelic(player);
                    break;
                case DataminerEffectKind.LoseRelic:
                    await LoseRandomRelic(player, subEffect.SecondaryAmount);
                    break;
                case DataminerEffectKind.DoublePowers:
                    await DoubleAllPowers(player, subEffect);
                    break;
                case DataminerEffectKind.OstyDamage:
                    if (cardPlay.Target != null && player.Osty is { IsAlive: true } osty)
                    {
                        int damage = osty.CurrentHp * subEffect.Amount;
                        await DamageCmd.Attack(damage).FromCard(this, cardPlay).Targeting(cardPlay.Target).Execute(choiceContext);
                    }
                    break;
                case DataminerEffectKind.InstantDeath:
                    await CreatureCmd.Kill(player.Creature, false);
                    break;
                case DataminerEffectKind.MoriyaDance:
                    MoriyaDance_Koishi.PlayDataminerDanceVideo(subEffect.PowerId);
                    break;
                case DataminerEffectKind.SpawnFumo:
                    SpawnFumo(player, subEffect.PowerId);
                    break;
                case DataminerEffectKind.ReplaceDeckWithIronWaves:
                    await ReplaceDeckWithIronWaves(player, subEffect.Amount);
                    break;
                case DataminerEffectKind.GodMode:
                    await EnableGodMode(choiceContext, player, subEffect.Amount == 0);
                    break;
                case DataminerEffectKind.ClearAllPowers:
                    await ClearPowers(player, subEffect, null);
                    break;
                case DataminerEffectKind.ClearEnemyDebuffs:
                    await ClearPowers(player, subEffect, PowerType.Debuff);
                    break;
                case DataminerEffectKind.ClearEnemyBuffs:
                    await ClearPowers(player, subEffect, PowerType.Buff);
                    break;
                case DataminerEffectKind.OverlayPlayers:
                case DataminerEffectKind.OverlayEnemies:
                    ApplyCreatureOverlays(player, subEffect);
                    break;
                case DataminerEffectKind.RandomVfx:
                case DataminerEffectKind.SpecificVfx:
                    PlayOfficialVfx(player, subEffect);
                    break;
                case DataminerEffectKind.ModifyHandLimit:
                    await PowerCmd.Apply<DataminerErrorPower>(
                        choiceContext,
                        player.Creature,
                        subEffect.Amount,
                        player.Creature,
                        this,
                        false);
                    break;
                case DataminerEffectKind.ApplyAllDebuffs:
                    await ApplyAllOfficialDebuffs(choiceContext, player, subEffect);
                    break;
            }
        }

        private async Task ApplyPowerToTargets(
            PlayerChoiceContext choiceContext,
            Player player,
            PowerModel power,
            DataminerSubEffect effect)
        {
            if (player.Creature.CombatState == null)
            {
                return;
            }

            List<Creature> targets = GetPowerTargets(player, effect);
            foreach (Creature target in targets)
            {
                await PowerCmd.Apply(
                    choiceContext,
                    power.ToMutable(),
                    target,
                    effect.Amount,
                    player.Creature,
                    this,
                    false);
            }
        }

        private static List<Creature> GetPowerTargets(Player player, DataminerSubEffect effect)
        {
            CombatState combatState = (CombatState)player.Creature.CombatState!;
            List<Creature> allies = combatState.PlayerCreatures
                .Where(creature => creature.IsAlive)
                .OrderBy(creature => creature.CombatId)
                .ToList();
            List<Creature> enemies = combatState.Enemies
                .Where(creature => creature.IsAlive)
                .OrderBy(creature => creature.CombatId)
                .ToList();

            return effect.PowerTarget switch
            {
                DataminerPowerTarget.Self => new List<Creature> { player.Creature },
                DataminerPowerTarget.Ally => SelectOneOrFallback(player, allies, effect.SecondaryAmount),
                DataminerPowerTarget.AllAllies => allies,
                DataminerPowerTarget.Enemy => SelectOne(enemies, effect.SecondaryAmount),
                DataminerPowerTarget.AllEnemies => enemies,
                DataminerPowerTarget.AllUnits => allies.Concat(enemies).ToList(),
                _ => new List<Creature> { player.Creature }
            };
        }

        private static List<Creature> SelectOneOrFallback(
            Player player,
            List<Creature> candidates,
            int selectionSeed)
        {
            List<Creature> otherAllies = candidates
                .Where(creature => creature != player.Creature)
                .ToList();
            if (otherAllies.Count == 0)
            {
                return new List<Creature> { player.Creature };
            }

            return new List<Creature> { SelectBySeed(otherAllies, selectionSeed) };
        }

        private static List<Creature> SelectOne(List<Creature> candidates, int selectionSeed)
        {
            if (candidates.Count == 0)
            {
                return new List<Creature>();
            }

            return new List<Creature> { SelectBySeed(candidates, selectionSeed) };
        }

        private static T SelectBySeed<T>(List<T> candidates, int selectionSeed) =>
            candidates[selectionSeed % candidates.Count];

        private static async Task SummonRandomMonsters(Player player, DataminerSubEffect effect)
        {
            if (effect.Amount <= 0 || string.IsNullOrEmpty(effect.PowerId) || player.Creature.CombatState == null)
            {
                return;
            }

            MonsterModel? monster = ModelDb.Monsters.FirstOrDefault(model => model.Id.Entry == effect.PowerId);
            if (monster == null)
            {
                return;
            }

            List<string?> slots = GetSummonSlots(player, effect.Amount, effect.SecondaryAmount);
            for (int i = 0; i < effect.Amount; i++)
            {
                Creature creature = await CreatureCmd.Add(
                    monster.ToMutable(),
                    player.Creature.CombatState,
                    CombatSide.Enemy,
                    i < slots.Count ? slots[i] : null);
                if (i >= slots.Count || slots[i] == null)
                {
                    PositionSummonedMonster(player, creature, i, effect.SecondaryAmount);
                }
            }
        }

        private static List<string?> GetSummonSlots(Player player, int count, int seed)
        {
            CombatState? combatState = player.Creature.CombatState as CombatState;
            if (combatState?.Encounter?.Slots == null)
            {
                return Enumerable.Repeat<string?>(null, count).ToList();
            }

            HashSet<string?> occupied = combatState.Enemies
                .Select(enemy => enemy.SlotName)
                .Where(slot => slot != null)
                .ToHashSet();
            List<string?> candidates = combatState.Encounter.Slots
                .Where(slot => !occupied.Contains(slot))
                .Cast<string?>()
                .ToList();
            List<string?> result = new();
            while (result.Count < count && candidates.Count > 0)
            {
                int index = (seed + result.Count * 97) % candidates.Count;
                string? slot = candidates[index];
                candidates.RemoveAt(index);
                result.Add(slot);
            }

            return result;
        }

        private static void PositionSummonedMonster(Player player, Creature summoned, int index, int seed)
        {
            MegaCrit.Sts2.Core.Nodes.Rooms.NCombatRoom? room = MegaCrit.Sts2.Core.Nodes.Rooms.NCombatRoom.Instance;
            MegaCrit.Sts2.Core.Nodes.Combat.NCreature? node = room?.GetCreatureNode(summoned);
            if (node == null)
            {
                return;
            }

            List<Creature> anchors = player.Creature.CombatState?.Enemies
                .Where(enemy => enemy != summoned && enemy.IsAlive)
                .ToList() ?? new List<Creature>();
            MegaCrit.Sts2.Core.Nodes.Combat.NCreature? anchorNode = anchors.Count == 0
                ? null
                : room?.GetCreatureNode(anchors[(seed + index) % anchors.Count]);
            Vector2 center = anchorNode?.GlobalPosition ?? node.GlobalPosition;
            float angle = Mathf.DegToRad((seed + index * 137) % 360);
            float radius = 280f + ((seed + index * 53) % 221);
            node.GlobalPosition = center + new Vector2(Mathf.Cos(angle), Mathf.Sin(angle) * 0.45f) * radius;
        }

        private static void SpawnFumo(Player player, string? textureName)
        {
            if (string.IsNullOrEmpty(textureName))
            {
                return;
            }

            NGiftYouFumoVfx? fumo = NGiftYouFumoVfx.Create(player.Creature, player.Creature, textureName);
            if (fumo != null)
            {
                MegaCrit.Sts2.Core.Nodes.Rooms.NCombatRoom.Instance?.CombatVfxContainer.AddChildSafely(fumo);
            }
        }

        private async Task ReplaceDeckWithIronWaves(Player player, int amount)
        {
            List<CardModel> deckCards = PileType.Deck.GetPile(player).Cards.ToList();
            if (deckCards.Count > 0)
            {
                await CardPileCmd.RemoveFromCombat(deckCards, true);
            }

            for (int i = 0; i < amount; i++)
            {
                CardModel ironWave = player.RunState.CreateCard<IronWave>(player);
                await AddCardToDeckSafely(ironWave);
            }
        }

        private static async Task AddCardToDeckSafely(CardModel card)
        {
            using (DataminerDeckProtectionPatch.AllowInternalGeneration())
            {
                await CardPileCmd.Add(card, PileType.Deck, CardPilePosition.Bottom, null, false);
            }
        }

        private async Task EnableGodMode(PlayerChoiceContext context, Player player, bool thisTurnOnly)
        {
            if (thisTurnOnly)
            {
                await PowerCmd.Apply<DataminerTurnGodModePower>(
                    context,
                    player.Creature,
                    1,
                    player.Creature,
                    this,
                    false);
                return;
            }

            const decimal godModeAmount = 999999999m;
            await PowerCmd.Apply<StrengthPower>(context, player.Creature, godModeAmount, player.Creature, null, false);
            await PowerCmd.Apply<BufferPower>(context, player.Creature, godModeAmount, player.Creature, null, false);
            await PowerCmd.Apply<RegenPower>(context, player.Creature, godModeAmount, player.Creature, null, false);
        }

        private static async Task ClearPowers(Player player, DataminerSubEffect effect, PowerType? type)
        {
            foreach (Creature target in GetPowerTargets(player, effect))
            {
                foreach (PowerModel power in target.Powers.ToList())
                {
                    if (type == null || power.TypeForCurrentAmount == type)
                    {
                        await PowerCmd.Remove(power);
                    }
                }
            }
        }

        private static void ApplyCreatureOverlays(Player player, DataminerSubEffect effect)
        {
            if (string.IsNullOrEmpty(effect.PowerId))
            {
                return;
            }

            foreach (Creature target in GetPowerTargets(player, effect))
            {
                NDataminerCreatureOverlayVfx.Create(target, effect.PowerId);
            }
        }

        private static void PlayOfficialVfx(Player player, DataminerSubEffect effect)
        {
            foreach (Creature target in GetPowerTargets(player, effect))
            {
                for (int i = 0; i < effect.Amount; i++)
                {
                    switch (effect.PowerId)
                    {
                        case "big_slash":
                            NBigSlashVfx? slash = NBigSlashVfx.Create(target);
                            if (slash != null)
                            {
                                MegaCrit.Sts2.Core.Nodes.Rooms.NCombatRoom.Instance?.CombatVfxContainer.AddChildSafely(slash);
                            }
                            break;
                        case "block_spark":
                            NBlockSparkVfx? spark = NBlockSparkVfx.Create(target);
                            if (spark != null)
                            {
                                MegaCrit.Sts2.Core.Nodes.Rooms.NCombatRoom.Instance?.CombatVfxContainer.AddChildSafely(spark);
                            }
                            break;
                        case "bounce_spark":
                            NBounceSparkVfx? bounce = NBounceSparkVfx.Create(target);
                            if (bounce != null)
                            {
                                MegaCrit.Sts2.Core.Nodes.Rooms.NCombatRoom.Instance?.CombatVfxContainer.AddChildSafely(bounce);
                            }
                            break;
                    }
                }
            }
        }

        private async Task ApplyAllOfficialDebuffs(
            PlayerChoiceContext context,
            Player player,
            DataminerSubEffect effect)
        {
            foreach (Creature target in GetPowerTargets(player, effect))
            {
                foreach (PowerModel debuff in DataminerBuffPool.GetOfficialDebuffs())
                {
                    await PowerCmd.Apply(
                        context,
                        debuff.ToMutable(),
                        target,
                        effect.Amount,
                        player.Creature,
                        this,
                        false);
                }
            }
        }

        private static async Task WinCombat(Player player)
        {
            if (player.Creature.CombatState == null)
            {
                return;
            }

            foreach (Creature enemy in player.Creature.CombatState.Enemies.ToList())
            {
                enemy.RemoveAllPowersInternalExcept(null);
                await CreatureCmd.Kill(enemy, false);
            }

            await CombatManager.Instance.CheckWinCondition();
        }

        private static async Task ChangeMaxHp(
            PlayerChoiceContext context,
            Player player,
            DataminerSubEffect effect)
        {
            foreach (Creature target in GetPowerTargets(player, effect))
            {
                if (effect.Amount > 0)
                {
                    await CreatureCmd.GainMaxHp(target, effect.Amount);
                }
                else if (effect.Amount < 0)
                {
                    await CreatureCmd.LoseMaxHp(context, target, -effect.Amount, true);
                }
            }
        }

        private static void AddExtraCardRewards(Player player, int count)
        {
            if (count <= 0 || player.RunState.CurrentRoom is not CombatRoom combatRoom)
            {
                return;
            }

            for (int i = 0; i < count; i++)
            {
                combatRoom.AddExtraReward(
                    player,
                    new CardReward(
                        CardCreationOptions.ForRoom(player, combatRoom.RoomType),
                        3,
                        player,
                        null));
            }
        }

        private static async Task ObtainRandomRelic(Player player)
        {
            RelicModel relic = RelicFactory.PullNextRelicFromFront(player).ToMutable();
            await RelicCmd.Obtain(relic, player, -1);
        }

        private static async Task LoseRandomRelic(Player player, int selectionSeed)
        {
            List<RelicModel> candidates = player.Relics
                .Where(relic => relic is not Dataminer_Koishi)
                .OrderBy(relic => relic.Id.Entry, StringComparer.Ordinal)
                .ToList();
            if (candidates.Count > 0)
            {
                await RelicCmd.Remove(SelectBySeed(candidates, selectionSeed));
            }
        }

        private static async Task DoubleAllPowers(Player player, DataminerSubEffect effect)
        {
            foreach (Creature target in GetPowerTargets(player, effect))
            {
                foreach (PowerModel power in target.Powers.ToList())
                {
                    if (power.Amount == 0)
                    {
                        continue;
                    }

                    await PowerCmd.ModifyAmount(
                        new ThrowingPlayerChoiceContext(),
                        power,
                        power.Amount,
                        player.Creature,
                        null,
                        false);
                }
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

        private static PileType GetPileType(DataminerPileScope? scope) => scope switch
        {
            DataminerPileScope.Draw => PileType.Draw,
            DataminerPileScope.Discard => PileType.Discard,
            DataminerPileScope.Exhaust => PileType.Exhaust,
            DataminerPileScope.Hand => PileType.Hand,
            _ => PileType.Hand
        };

        private static async Task LoseHealthByPileCount(PlayerChoiceContext context, Player player)
        {
            int count = PileType.Hand.GetPile(player).Cards.Count
                + PileType.Discard.GetPile(player).Cards.Count
                + PileType.Draw.GetPile(player).Cards.Count
                + PileType.Deck.GetPile(player).Cards.Count
                + PileType.Exhaust.GetPile(player).Cards.Count;
            if (count > 0)
            {
                await CreatureCmd.Damage(
                    context,
                    player.Creature,
                    count,
                    ValueProp.Unblockable | ValueProp.Unpowered,
                    null,
                    null);
            }
        }

        private static async Task AddSoulsToPile(Player player, PileType pileType, int amount)
        {
            if (amount <= 0 || pileType == PileType.Deck)
            {
                return;
            }

            List<Soul> souls = Soul.Create(player, amount, player.Creature.CombatState!).ToList();
            await CardPileCmd.AddGeneratedCardsToCombat(souls, pileType, player, CardPilePosition.Bottom);
        }

        private static async Task RetrieveCards(
            PlayerChoiceContext context,
            Player player,
            PileType sourcePile,
            int amount,
            DataminerPileScope? destinationScope)
        {
            if (amount <= 0 || sourcePile.GetPile(player).Cards.Count == 0)
            {
                return;
            }

            CardSelectorPrefs prefs = new CardSelectorPrefs(
                new MegaCrit.Sts2.Core.Localization.LocString(
                    "gameplay_ui",
                    "KOMEIJIKOISHI_DATAMINER_ERROR_PROMPT"),
                0,
                amount);
            IEnumerable<CardModel> selected = await CardSelectCmd.FromCombatPile(
                context,
                sourcePile.GetPile(player),
                player,
                prefs);
            PileType destination = GetPileType(destinationScope);
            foreach (CardModel card in selected.ToList())
            {
                await CardPileCmd.Add(card, destination, CardPilePosition.Bottom, null, false);
            }
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
                    await AddCardToDeckSafely(generatedCard);
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
            EnsureEffectLoaded();
            CardLocation location = base.GetResultLocationForCardPlay();
            location.pileType = GetResultPileForCardType();
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
            return GetResultPileForCardType();
        }
#endif

        private PileType GetResultPileForCardType()
        {
            EnsureEffectLoaded();
            return effect.CardType == CardType.Power || RemovesThisCardFromCombatAfterPlay()
                ? PileType.None
                : GetStablePile(effect.ReturnPile);
        }

        private bool RemovesThisCardFromCombatAfterPlay()
        {
            if (effect.Primary.Kind == DataminerEffectKind.ReplaceDeckWithIronWaves)
            {
                return true;
            }

            return effect.ExtraEffect.Kind == DataminerEffectKind.ReplaceDeckWithIronWaves
                && base.Owner is Player player
                && effect.ExtraCondition.IsMet(player);
        }
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
