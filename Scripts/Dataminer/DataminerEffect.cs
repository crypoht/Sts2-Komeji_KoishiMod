using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Serialization;
using KomeijiKoishi.Cards;
using KomeijiKoishi.Vfx;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Combat.History;
using MegaCrit.Sts2.Core.Combat.History.Entries;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.Runs;

namespace KomeijiKoishi.Dataminer
{
    public enum DataminerEffectKind
    {
        None, Damage, DamageAll, Block, AllyBlock, Draw, Energy, Health,
        ExhaustRandomHand, EnemyBlock, RandomPower, RandomBuff, GenerateCards, Gold, Potions,
        DamagePerPile, ChannelOrb, Combo, DiscardAllHand, ExhaustAllHand,
        ExhaustSelectedHand, AutoPlayPile, Poison, UpgradePile, UpgradeAll,
        Summon, Stars, EnchantPile, RandomizeHandCost,
        DirectWin, MaxHp, ExtraCardRewards, ObtainRelic, LoseRelic,
        DoublePowers, OstyDamage, SpecificPower,
        InstantDeath, MoriyaDance, SpawnFumo, ReplaceDeckWithIronWaves, GodMode,
        ClearAllPowers, ClearEnemyDebuffs, ClearEnemyBuffs,
        OverlayPlayers, OverlayEnemies, RandomVfx, SpecificVfx,
        ModifyHandLimit, ApplyAllDebuffs, RepeatPrimary,
        HealthLossByPileCount, GenerateShivs, GenerateSouls,
        GenerateSoulsToPile, RetrieveDiscardCards, RetrieveDrawCards
    }

    public enum DataminerGeneratedCardKind { SpecificDataminer, Random }

    public enum DataminerGeneratedCardDestination { Hand, Draw, Discard, Exhaust, Deck }

    public enum DataminerPileScope { Deck, Exhaust, Draw, Discard, Hand, All }

    public enum DataminerOrbKind { Lightning, Glass, Dark, Frost }

    public enum DataminerReturnPile { None, DrawTop, DrawBottom, Hand, Exhaust }

    public enum DataminerPowerTarget
    {
        Self,
        Ally,
        AllAllies,
        Enemy,
        AllEnemies,
        AllUnits
    }

    public enum DataminerConditionKind
    {
        None, HandAtLeast, HandAtMost, DrawPileEmpty, DiscardPileEmpty,
        EnemiesAtLeast, AlliesAtLeast, ExhaustPileAtLeast, PlayedAttacksAtLeast,
        PlayedSkillsAtLeast, PowersAtLeast, PowersAtMost, LostHealthAtLeast,
        PlayedEtherealAtLeast, EnemyDebuffsAtLeast, PotionsAtLeast, GoldAtLeast
    }

    public enum DataminerAbilityTriggerKind
    {
        None,
        OnLoseHealth,
        OnExhaustCard,
        OnGiveVulnerable,
        OnTurnEnd,
        OnTurnStart,
        OnCombatEnd,
        OnDrawStrike,
        OnGainBlock,
        OnDrawCard,
        OnPlayCard,
        OnPlaySkill,
        OnPlayAttack,
        OnPlayPower,
        OnPlayStatus,
        OnPlayCurse,
        OnUnblockedDamage,
        OnGenerateCard,
        OnSpendOrGainStars,
        OnSpendEnergy,
        OnAttack,
        OnGiveEnemyDebuff,
        OnPlaySoul,
        OnDrawEthereal,
        OnPlayEthereal,
        OnDrawStatus,
        OnChannelLightning,
        OnGenerateStatus,
        OnShuffleDrawPile,
        EveryDrawCount
    }

    public readonly struct DataminerCondition
    {
        [JsonConstructor]
        public DataminerCondition(DataminerConditionKind kind, int amount)
        {
            Kind = kind;
            Amount = amount;
        }

        public DataminerConditionKind Kind { get; }
        public int Amount { get; }

        public bool IsMet(Player player)
        {
            CombatHistory? history = CombatManager.Instance.History;
            return Kind switch
            {
                DataminerConditionKind.None => true,
                DataminerConditionKind.HandAtLeast => PileType.Hand.GetPile(player).Cards.Count >= Amount,
                DataminerConditionKind.HandAtMost => PileType.Hand.GetPile(player).Cards.Count <= Amount,
                DataminerConditionKind.DrawPileEmpty => PileType.Draw.GetPile(player).Cards.Count == 0,
                DataminerConditionKind.DiscardPileEmpty => PileType.Discard.GetPile(player).Cards.Count == 0,
                DataminerConditionKind.EnemiesAtLeast => player.Creature.CombatState?.Enemies.Count(e => e.IsAlive) >= Amount,
                DataminerConditionKind.AlliesAtLeast => player.Creature.CombatState?.PlayerCreatures.Count(c => c.IsAlive) >= Amount,
                DataminerConditionKind.ExhaustPileAtLeast => PileType.Exhaust.GetPile(player).Cards.Count >= Amount,
                DataminerConditionKind.PlayedAttacksAtLeast => CountPlayed(history, player, CardType.Attack) >= Amount,
                DataminerConditionKind.PlayedSkillsAtLeast => CountPlayed(history, player, CardType.Skill) >= Amount,
                DataminerConditionKind.PowersAtLeast => player.Creature.Powers.Count >= Amount,
                DataminerConditionKind.PowersAtMost => player.Creature.Powers.Count <= Amount,
                DataminerConditionKind.LostHealthAtLeast => CountLostHealth(history, player) >= Amount,
                DataminerConditionKind.PlayedEtherealAtLeast => history?.CardPlaysFinished.Count(e => e.CardPlay.Card.Owner == player && e.WasEthereal) >= Amount,
                DataminerConditionKind.EnemyDebuffsAtLeast => HasEnemyDebuffs(player, Amount),
                DataminerConditionKind.PotionsAtLeast => player.Potions.Count(p => p != null) >= Amount,
                DataminerConditionKind.GoldAtLeast => player.Gold >= Amount,
                _ => true
            };
        }

        public static DataminerCondition Create(Player owner, bool allowNone)
        {
            var rng = owner.RunState.Rng.CombatCardGeneration;
            if (allowNone && rng.NextInt(2) == 0)
            {
                return new DataminerCondition(DataminerConditionKind.None, 0);
            }

            DataminerConditionKind kind = (DataminerConditionKind)rng.NextInt(1, (int)DataminerConditionKind.GoldAtLeast + 1);
            return CreateForConsole(owner, kind);
        }

        public static DataminerCondition CreateForConsole(Player owner, DataminerConditionKind kind)
        {
            if (kind == DataminerConditionKind.None)
            {
                return new DataminerCondition(DataminerConditionKind.None, 0);
            }

            var rng = owner.RunState.Rng.CombatCardGeneration;
            int amount = kind switch
            {
                DataminerConditionKind.HandAtLeast or DataminerConditionKind.HandAtMost => rng.NextInt(0, 6),
                DataminerConditionKind.EnemiesAtLeast or DataminerConditionKind.AlliesAtLeast => rng.NextInt(1, 4),
                DataminerConditionKind.ExhaustPileAtLeast => rng.NextInt(0, 6),
                DataminerConditionKind.PlayedAttacksAtLeast or DataminerConditionKind.PlayedSkillsAtLeast => rng.NextInt(1, 6),
                DataminerConditionKind.PowersAtLeast => rng.NextInt(1, 5),
                DataminerConditionKind.PowersAtMost => rng.NextInt(3, 8),
                DataminerConditionKind.LostHealthAtLeast => rng.NextInt(0, 5),
                DataminerConditionKind.PlayedEtherealAtLeast => rng.NextInt(0, 3),
                DataminerConditionKind.EnemyDebuffsAtLeast => rng.NextInt(1, 4),
                DataminerConditionKind.PotionsAtLeast => rng.NextInt(1, 3),
                DataminerConditionKind.GoldAtLeast => rng.NextInt(100, 201),
                _ => 0
            };
            return new DataminerCondition(kind, amount);
        }

        private static int CountPlayed(CombatHistory? history, Player player, CardType type) =>
            history?.CardPlaysFinished.Count(e => e.CardPlay.Card.Owner == player && e.CardPlay.Card.Type == type) ?? 0;

        private static int CountLostHealth(CombatHistory? history, Player player) =>
            history?.Entries.OfType<DamageReceivedEntry>().Count(e => e.Receiver == player.Creature && e.Result.UnblockedDamage > 0) ?? 0;

        private static bool IsDebuff(PowerModel power) =>
            power.TypeForCurrentAmount == PowerType.Debuff && power is not ITemporaryPower;

        private static bool HasEnemyDebuffs(Player player, int amount)
        {
            return player.Creature.CombatState?.Enemies.Any(e => e.IsAlive && e.Powers.Count(IsDebuff) >= amount) == true;
        }
    }

    public readonly struct DataminerSubEffect
    {
        [JsonConstructor]
        public DataminerSubEffect(
            DataminerEffectKind kind,
            int amount,
            string? powerId,
            DataminerGeneratedCardKind? generatedCardKind = null,
            string? generatedCardId = null,
            DataminerGeneratedCardDestination? generatedCardDestination = null,
            DataminerPileScope? pileScope = null,
            int secondaryAmount = 0,
            DataminerPowerTarget powerTarget = DataminerPowerTarget.Self)
        {
            Kind = kind;
            Amount = amount;
            PowerId = powerId;
            GeneratedCardKind = generatedCardKind;
            GeneratedCardId = generatedCardId;
            GeneratedCardDestination = generatedCardDestination;
            PileScope = pileScope;
            SecondaryAmount = secondaryAmount;
            PowerTarget = powerTarget;
        }

        public DataminerEffectKind Kind { get; }
        public int Amount { get; }
        public string? PowerId { get; }
        public DataminerGeneratedCardKind? GeneratedCardKind { get; }
        public string? GeneratedCardId { get; }
        public DataminerGeneratedCardDestination? GeneratedCardDestination { get; }
        public DataminerPileScope? PileScope { get; }
        public int SecondaryAmount { get; }
        public DataminerPowerTarget PowerTarget { get; }

        public static DataminerSubEffect None => new(DataminerEffectKind.None, 0, null);
    }

    public readonly struct DataminerEffect
    {
        [JsonConstructor]
        public DataminerEffect(
            CardType cardType, int cost, DataminerSubEffect primary,
            DataminerCondition playCondition, DataminerCondition extraCondition,
            DataminerSubEffect extraEffect, DataminerReturnPile returnPile,
            int extraPrimaryRepeats, bool autoPlayAtTurnEnd, int comboAmount = 0,
            bool unplayable = false, bool mustPlayFirst = false)
        {
            CardType = cardType;
            Cost = cost;
            Primary = primary;
            PlayCondition = playCondition;
            ExtraCondition = extraCondition;
            ExtraEffect = extraEffect;
            ReturnPile = returnPile;
            ExtraPrimaryRepeats = extraPrimaryRepeats;
            AutoPlayAtTurnEnd = autoPlayAtTurnEnd;
            ComboAmount = comboAmount;
            Unplayable = unplayable;
            MustPlayFirst = mustPlayFirst;
        }

        public CardType CardType { get; }
        public int Cost { get; }
        public DataminerSubEffect Primary { get; }
        public DataminerCondition PlayCondition { get; }
        public DataminerCondition ExtraCondition { get; }
        public DataminerSubEffect ExtraEffect { get; }
        public DataminerReturnPile ReturnPile { get; }
        public int ExtraPrimaryRepeats { get; }
        public bool AutoPlayAtTurnEnd { get; }
        public int ComboAmount { get; }
        public bool Unplayable { get; }
        public bool MustPlayFirst { get; }
        public DataminerAbilityTriggerKind AbilityTrigger { get; init; }
        public DataminerSubEffect AbilityEffect { get; init; }
        public DataminerEffectKind Kind => Primary.Kind;
        public int Amount => Primary.Amount;
        public string? PowerId => Primary.PowerId;

        public TargetType TargetType => CardType == CardType.Power
            ? TargetType.Self
            : Primary.Kind switch
        {
            DataminerEffectKind.Damage or DataminerEffectKind.DamagePerPile or DataminerEffectKind.EnemyBlock or DataminerEffectKind.Poison or DataminerEffectKind.OstyDamage => TargetType.AnyEnemy,
            DataminerEffectKind.DamageAll => TargetType.AllEnemies,
            DataminerEffectKind.AllyBlock => TargetType.AnyAlly,
            _ => TargetType.Self
        };

        public static DataminerEffect Create(Player owner)
        {
            var rng = owner.RunState.Rng.CombatCardGeneration;
            CardType cardType = new[] { CardType.Attack, CardType.Skill, CardType.Status, CardType.Curse, CardType.Power }[rng.NextInt(5)];
            DataminerAbilityTriggerKind abilityTrigger = DataminerAbilityTriggerKind.None;
            DataminerSubEffect abilityEffect = DataminerSubEffect.None;
            if (cardType == CardType.Power)
            {
                abilityTrigger = (DataminerAbilityTriggerKind)rng.NextInt(0, (int)DataminerAbilityTriggerKind.EveryDrawCount + 1);
                abilityEffect = CreateAbilitySubEffect(owner);
            }
            DataminerSubEffect primary = CreateSubEffect(owner, null, false);
            DataminerCondition playCondition = CreatePlayCondition(owner);
            DataminerCondition extraCondition = DataminerCondition.Create(owner, true);
            DataminerSubEffect extraEffect = rng.NextInt(2) == 0
                ? DataminerSubEffect.None
                : CreateSubEffect(owner, null, true);
            int comboAmount = rng.NextInt(100) < 20 ? rng.NextInt(1, 5) : 0;
            DataminerReturnPile returnPile = rng.NextInt(10) < 7
                ? DataminerReturnPile.None
                : (DataminerReturnPile)rng.NextInt(1, 5);
            bool autoPlayAtTurnEnd = rng.NextInt(2) == 0;
            bool unplayable = rng.NextInt(10) == 0;
            bool mustPlayFirst = !unplayable && rng.NextInt(40) == 0;
            DataminerEffect result = new DataminerEffect(cardType, rng.NextInt(5), primary, playCondition, extraCondition, extraEffect, returnPile, 0, autoPlayAtTurnEnd, comboAmount, unplayable, mustPlayFirst)
            {
                AbilityTrigger = abilityTrigger,
                AbilityEffect = abilityEffect
            };
            return result;
        }

        private static DataminerSubEffect CreateAbilitySubEffect(Player owner)
        {
            var rng = owner.RunState.Rng.CombatCardGeneration;
            DataminerEffectKind[] pool =
            {
                DataminerEffectKind.Damage, DataminerEffectKind.Block,
                DataminerEffectKind.Draw, DataminerEffectKind.Energy,
                DataminerEffectKind.Health, DataminerEffectKind.RandomPower,
                DataminerEffectKind.Poison, DataminerEffectKind.InstantDeath,
                DataminerEffectKind.DirectWin, DataminerEffectKind.GenerateShivs,
                DataminerEffectKind.GenerateSouls, DataminerEffectKind.ChannelOrb
            };
            return CreateSubEffect(owner, pool[rng.NextInt(pool.Length)], true);
        }

        public static DataminerEffect CreateForConsole(
            Player owner,
            DataminerConditionKind playConditionKind,
            DataminerEffectKind primaryKind,
            DataminerConditionKind extraConditionKind,
            DataminerEffectKind extraEffectKind,
            int? forcedCost = null,
            CardType? forcedCardType = null,
            DataminerReturnPile returnPile = DataminerReturnPile.None,
            bool autoPlayAtTurnEnd = false,
            DataminerAbilityTriggerKind abilityTrigger = DataminerAbilityTriggerKind.None,
            DataminerSubEffect abilityEffect = default)
        {
            var rng = owner.RunState.Rng.CombatCardGeneration;
            CardType cardType = forcedCardType ?? new[] { CardType.Attack, CardType.Skill, CardType.Status, CardType.Curse }[rng.NextInt(4)];
            DataminerSubEffect primary = CreateSubEffect(owner, primaryKind);
            DataminerCondition playCondition = DataminerCondition.CreateForConsole(owner, playConditionKind);
            DataminerCondition extraCondition = DataminerCondition.CreateForConsole(owner, extraConditionKind);
            DataminerSubEffect extraEffect = CreateSubEffect(owner, extraEffectKind);
            DataminerEffect result = new DataminerEffect(
                cardType,
                forcedCost ?? rng.NextInt(5),
                primary,
                playCondition,
                extraCondition,
                extraEffect,
                cardType == CardType.Power ? DataminerReturnPile.None : returnPile,
                0,
                autoPlayAtTurnEnd)
            {
                AbilityTrigger = cardType == CardType.Power ? abilityTrigger : DataminerAbilityTriggerKind.None,
                AbilityEffect = cardType == CardType.Power ? abilityEffect : DataminerSubEffect.None
            };
            return result;
        }

        public static DataminerSubEffect CreateAbilityEffectForConsole(
            Player owner,
            DataminerEffectKind kind)
        {
            return CreateSubEffect(owner, kind, true);
        }

        private static DataminerCondition CreatePlayCondition(Player owner)
        {
            var rng = owner.RunState.Rng.CombatCardGeneration;
            return rng.NextInt(10) < 9 ? new DataminerCondition(DataminerConditionKind.None, 0) : DataminerCondition.Create(owner, false);
        }

        private static DataminerSubEffect CreateSubEffect(
            Player owner,
            DataminerEffectKind? requestedKind = null,
            bool allowRepeat = false)
        {
            var rng = owner.RunState.Rng.CombatCardGeneration;
            if (requestedKind == DataminerEffectKind.None)
            {
                return DataminerSubEffect.None;
            }

            DataminerEffectKind kind = requestedKind ?? DataminerEffectKind.None;
            if (requestedKind == null)
            {
                int max = (int)DataminerEffectKind.RepeatPrimary;
                kind = (DataminerEffectKind)rng.NextInt(1, max + 1);
                while (kind == DataminerEffectKind.Combo
                    || (!allowRepeat && kind == DataminerEffectKind.RepeatPrimary))
                {
                    kind = (DataminerEffectKind)rng.NextInt(1, max + 1);
                }
            }

            string? powerId = null;
            DataminerGeneratedCardKind? generatedCardKind = null;
            string? generatedCardId = null;
            DataminerGeneratedCardDestination? generatedCardDestination = null;
            DataminerPileScope? pileScope = null;
            int secondaryAmount = 0;
            DataminerPowerTarget powerTarget = DataminerPowerTarget.Self;

            int amount = kind switch
            {
                DataminerEffectKind.Energy => rng.NextInt(-4, 6),
                DataminerEffectKind.Health => rng.NextInt(-20, 11),
                DataminerEffectKind.RandomPower or DataminerEffectKind.SpecificPower => rng.NextInt(-5, 4),
                DataminerEffectKind.RandomBuff => rng.NextInt(1, 11),
                DataminerEffectKind.Gold => rng.NextInt(-100, 91),
                DataminerEffectKind.Potions => rng.NextInt(0, 3),
                DataminerEffectKind.GenerateCards => rng.NextInt(0, 3),
                DataminerEffectKind.DamagePerPile => rng.NextInt(1, 5),
                DataminerEffectKind.ChannelOrb => rng.NextInt(0, 6),
                DataminerEffectKind.Combo => rng.NextInt(1, 5),
                DataminerEffectKind.ExhaustSelectedHand => rng.NextInt(1, 7),
                DataminerEffectKind.AutoPlayPile => rng.NextInt(0, 4),
                DataminerEffectKind.Draw => rng.NextInt(0, 7),
                DataminerEffectKind.UpgradePile => rng.NextInt(0, 9),
                DataminerEffectKind.Summon => rng.NextInt(1, 4),
                DataminerEffectKind.Stars => rng.NextInt(1, 11),
                DataminerEffectKind.EnchantPile => rng.NextInt(0, 7),
                DataminerEffectKind.RandomizeHandCost => rng.NextInt(0, 5),
                DataminerEffectKind.MaxHp => rng.NextInt(-10, 6),
                DataminerEffectKind.ExtraCardRewards => rng.NextInt(1, 4),
                DataminerEffectKind.OstyDamage => rng.NextInt(1, 4),
                DataminerEffectKind.ReplaceDeckWithIronWaves => rng.NextInt(5, 11),
                DataminerEffectKind.GodMode => rng.NextInt(2),
                DataminerEffectKind.RandomVfx or DataminerEffectKind.SpecificVfx => rng.NextInt(1, 5),
                DataminerEffectKind.ModifyHandLimit => rng.NextInt(2) == 0 ? rng.NextInt(1, 11) : -rng.NextInt(1, 6),
                DataminerEffectKind.ApplyAllDebuffs => rng.NextInt(1, 3),
                DataminerEffectKind.RepeatPrimary => rng.NextInt(1, 3),
                DataminerEffectKind.HealthLossByPileCount => 0,
                DataminerEffectKind.GenerateShivs or DataminerEffectKind.GenerateSouls
                    or DataminerEffectKind.GenerateSoulsToPile => rng.NextInt(1, 7),
                DataminerEffectKind.RetrieveDiscardCards or DataminerEffectKind.RetrieveDrawCards => rng.NextInt(1, 5),
                DataminerEffectKind.DirectWin or DataminerEffectKind.ObtainRelic or DataminerEffectKind.LoseRelic
                    or DataminerEffectKind.DoublePowers or DataminerEffectKind.InstantDeath
                    or DataminerEffectKind.MoriyaDance or DataminerEffectKind.SpawnFumo
                    or DataminerEffectKind.ClearAllPowers or DataminerEffectKind.ClearEnemyDebuffs
                    or DataminerEffectKind.ClearEnemyBuffs or DataminerEffectKind.OverlayPlayers
                    or DataminerEffectKind.OverlayEnemies => 0,
                DataminerEffectKind.None => 0,
                _ => rng.NextInt(1, 31)
            };

            if (kind is DataminerEffectKind.RandomPower or DataminerEffectKind.SpecificPower)
            {
                powerTarget = (DataminerPowerTarget)rng.NextInt(6);
                powerId = DataminerPowerPool.SelectRandomPowerId(
                    owner,
                    !TargetsEnemies(powerTarget),
                    amount >= 0);
                secondaryAmount = rng.NextInt(1_000_000);
            }
            else if (kind == DataminerEffectKind.RandomBuff)
            {
                powerTarget = (DataminerPowerTarget)rng.NextInt(6);
                powerId = DataminerBuffPool.SelectRandomBuffId(
                    owner,
                    !TargetsEnemies(powerTarget),
                    amount >= 0);
                secondaryAmount = rng.NextInt(1_000_000);
            }
            else if (kind == DataminerEffectKind.MaxHp)
            {
                powerTarget = (DataminerPowerTarget)rng.NextInt(3);
                secondaryAmount = rng.NextInt(1_000_000);
            }
            else if (kind == DataminerEffectKind.DoublePowers)
            {
                powerTarget = (DataminerPowerTarget)rng.NextInt(6);
                secondaryAmount = rng.NextInt(1_000_000);
            }
            else if (kind == DataminerEffectKind.LoseRelic)
            {
                secondaryAmount = rng.NextInt(1_000_000);
            }
            else if (kind == DataminerEffectKind.GenerateCards)
            {
                generatedCardKind = rng.NextInt(2) == 0 ? DataminerGeneratedCardKind.SpecificDataminer : DataminerGeneratedCardKind.Random;
                generatedCardDestination = (DataminerGeneratedCardDestination)rng.NextInt(5);
                if (generatedCardKind == DataminerGeneratedCardKind.Random)
                {
                    generatedCardId = SelectRandomCardId(owner);
                }
            }
            else if (kind is DataminerEffectKind.DamagePerPile or DataminerEffectKind.AutoPlayPile or DataminerEffectKind.UpgradePile or DataminerEffectKind.EnchantPile)
            {
                pileScope = (DataminerPileScope)rng.NextInt(5);
                if (kind == DataminerEffectKind.EnchantPile)
                {
                    powerId = DataminerEnchantmentPool.SelectRandomEnchantmentId(owner);
                    secondaryAmount = rng.NextInt(1, 7);
                }
            }
            else if (kind == DataminerEffectKind.ChannelOrb)
            {
                powerId = ((DataminerOrbKind)rng.NextInt(4)).ToString();
            }
            else if (kind == DataminerEffectKind.Summon)
            {
                powerId = SelectRandomMonsterId(owner);
                secondaryAmount = rng.NextInt(360);
            }
            else if (kind == DataminerEffectKind.MoriyaDance)
            {
                powerId = MoriyaDance_Koishi.GetRandomDanceVideoPath(owner);
            }
            else if (kind == DataminerEffectKind.SpawnFumo)
            {
                powerId = owner.RunState.Rng.CombatCardGeneration.NextItem(NGiftYouFumoVfx.TextureNames);
            }
            else if (kind is DataminerEffectKind.OverlayPlayers or DataminerEffectKind.OverlayEnemies)
            {
                powerId = GetDataminerPortraitPath(owner.RunState.Rng.CombatCardGeneration.NextInt(1, 8));
                powerTarget = kind == DataminerEffectKind.OverlayPlayers
                    ? (DataminerPowerTarget)rng.NextInt(3)
                    : (rng.NextInt(2) == 0 ? DataminerPowerTarget.Enemy : DataminerPowerTarget.AllEnemies);
                secondaryAmount = rng.NextInt(1_000_000);
            }
            else if (kind is DataminerEffectKind.RandomVfx or DataminerEffectKind.SpecificVfx)
            {
                powerId = SelectVfxId(owner);
                powerTarget = (DataminerPowerTarget)rng.NextInt(6);
                secondaryAmount = rng.NextInt(1_000_000);
            }
            else if (kind == DataminerEffectKind.ClearAllPowers)
            {
                powerTarget = (DataminerPowerTarget)rng.NextInt(3);
                secondaryAmount = rng.NextInt(1_000_000);
            }
            else if (kind is DataminerEffectKind.ClearEnemyDebuffs or DataminerEffectKind.ClearEnemyBuffs)
            {
                powerTarget = rng.NextInt(2) == 0 ? DataminerPowerTarget.Enemy : DataminerPowerTarget.AllEnemies;
                secondaryAmount = rng.NextInt(1_000_000);
            }
            else if (kind == DataminerEffectKind.ApplyAllDebuffs)
            {
                powerTarget = (DataminerPowerTarget)rng.NextInt(3);
                secondaryAmount = rng.NextInt(1_000_000);
            }
            else if (kind is DataminerEffectKind.GenerateSoulsToPile
                or DataminerEffectKind.RetrieveDiscardCards
                or DataminerEffectKind.RetrieveDrawCards)
            {
                pileScope = (DataminerPileScope)rng.NextInt(5);
            }

            return new DataminerSubEffect(kind, amount, powerId, generatedCardKind, generatedCardId, generatedCardDestination, pileScope, secondaryAmount, powerTarget);
        }

        private static bool TargetsEnemies(DataminerPowerTarget target) =>
            target is DataminerPowerTarget.Enemy
                or DataminerPowerTarget.AllEnemies
                or DataminerPowerTarget.AllUnits;

        private static string? SelectRandomCardId(Player owner)
        {
            List<CardModel> pool = ModelDb.AllCards
                .Where(card => card is not DataminerCard_Koishi)
                .Where(card => card.Type != CardType.Power && card.Type != CardType.Quest && card.Rarity != CardRarity.Quest)
                .Where(card => card.CanBeGeneratedByModifiers)
                .OrderBy(card => card.Id.Entry, StringComparer.Ordinal)
                .ToList();
            return pool.Count == 0 ? null : owner.RunState.Rng.CombatCardGeneration.NextItem(pool)?.Id.Entry;
        }

        private static string? SelectRandomMonsterId(Player owner)
        {
            List<MonsterModel> pool = ModelDb.Monsters
                .OrderBy(monster => monster.Id.Entry, StringComparer.Ordinal)
                .ToList();
            return pool.Count == 0 ? null : owner.RunState.Rng.CombatCardGeneration.NextItem(pool)?.Id.Entry;
        }

        private static string GetDataminerPortraitPath(int variant) =>
            $"res://mods/Komeiji_Koishi/images/error/DataminerCard_Koishi_{variant}.png";

        private static string? SelectVfxId(Player owner)
        {
            string[] ids = { "big_slash", "block_spark", "bounce_spark" };
            return owner.RunState.Rng.CombatCardGeneration.NextItem(ids);
        }
    }
}
