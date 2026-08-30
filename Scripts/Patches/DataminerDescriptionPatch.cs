using BaseLib.Patches.Localization;
using KomeijiKoishi.Cards;
using KomeijiKoishi.Dataminer;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Nodes.Cards;
using MegaCrit.Sts2.Core.Models;

namespace KomeijiKoishi.Patches;

public static class DataminerDescriptionPatch
{
    private static bool _registered;

    public static bool ShowReadableDescription { get; private set; }

    public static void Register()
    {
        if (_registered)
        {
            return;
        }

        DescriptionOverrides.CustomizeDescription += CustomizeDescription;
        _registered = true;
    }

    public static void SetReadable(bool enabled, Player? player = null)
    {
        ShowReadableDescription = enabled;
        RefreshVisibleCards(player);
    }

    private static void CustomizeDescription(CardModel card, Creature? target, ref string description)
    {
        if (!ShowReadableDescription || card is not DataminerCard_Koishi dataminer)
        {
            return;
        }

        description = BuildDescription(dataminer.GetEffectForDescription());
    }

    private static void RefreshVisibleCards(Player? issuingPlayer)
    {
        IEnumerable<Player> players = issuingPlayer?.Creature.CombatState?.Players
            ?? Array.Empty<Player>();

        foreach (Player player in players.Distinct())
        {
            foreach (CardModel card in player.Piles.SelectMany(pile => pile.Cards))
            {
                if (card is not DataminerCard_Koishi)
                {
                    continue;
                }

                NCard? cardNode = NCard.FindOnTable(card);
                if (cardNode != null)
                {
                    cardNode.UpdateVisuals(cardNode.DisplayingPile, CardPreviewMode.Normal);
                }
            }
        }
    }

    private static string BuildDescription(DataminerEffect effect)
    {
        if (effect.CardType == CardType.Power)
        {
            return DataminerDescriptionPatch.ShowReadableDescription
                ? $"触发条件：{BuildAbilityTrigger(effect.AbilityTrigger)}。\n{BuildSubEffect(effect.AbilityEffect)}"
                : "错误";
        }

        List<string> lines = new()
        {
            BuildSubEffect(effect.Primary)
        };

        if (effect.PlayCondition.Kind != DataminerConditionKind.None)
        {
            lines.Insert(0, $"满足条件：{BuildCondition(effect.PlayCondition)}。");
        }

        if (effect.ExtraEffect.Kind != DataminerEffectKind.None
            && effect.ExtraCondition.Kind != DataminerConditionKind.None)
        {
            lines.Add($"若{BuildCondition(effect.ExtraCondition)}，则{BuildSubEffect(effect.ExtraEffect)}");
        }
        else if (effect.ExtraEffect.Kind != DataminerEffectKind.None)
        {
            lines.Add($"额外效果：{BuildSubEffect(effect.ExtraEffect)}");
        }

        if (effect.ComboAmount > 0)
        {
            lines.Add($"增加 {effect.ComboAmount} 次连击。");
        }

        if (effect.ReturnPile != DataminerReturnPile.None)
        {
            lines.Add(effect.ReturnPile switch
            {
                DataminerReturnPile.DrawTop => "打出后回到抽牌堆顶。",
                DataminerReturnPile.DrawBottom => "打出后回到抽牌堆底。",
                DataminerReturnPile.Hand => "打出后回到手牌。",
                DataminerReturnPile.Exhaust => "打出后进入消耗牌堆。",
                _ => string.Empty
            });
        }

        if (effect.AutoPlayAtTurnEnd)
        {
            lines.Add("回合结束时自动打出。");
        }

        if (effect.Unplayable)
        {
            lines.Add("不能被打出。");
        }
        else if (effect.MustPlayFirst)
        {
            lines.Add("必须优先打出这张牌。");
        }

        return string.Join("\n", lines.Where(line => !string.IsNullOrWhiteSpace(line)));
    }

    private static string BuildCondition(DataminerCondition condition)
    {
        return condition.Kind switch
        {
            DataminerConditionKind.None => "无条件",
            DataminerConditionKind.HandAtLeast => $"手牌不少于 {condition.Amount} 张",
            DataminerConditionKind.HandAtMost => $"手牌不多于 {condition.Amount} 张",
            DataminerConditionKind.DrawPileEmpty => "抽牌堆为空",
            DataminerConditionKind.DiscardPileEmpty => "弃牌堆为空",
            DataminerConditionKind.EnemiesAtLeast => $"场上敌人不少于 {condition.Amount} 只",
            DataminerConditionKind.AlliesAtLeast => $"场上队友不少于 {condition.Amount} 名",
            DataminerConditionKind.ExhaustPileAtLeast => $"消耗牌堆至少有 {condition.Amount} 张牌",
            DataminerConditionKind.PlayedAttacksAtLeast => $"本场战斗至少打出过 {condition.Amount} 张攻击牌",
            DataminerConditionKind.PlayedSkillsAtLeast => $"本场战斗至少打出过 {condition.Amount} 张技能牌",
            DataminerConditionKind.PowersAtLeast => $"拥有至少 {condition.Amount} 个能力",
            DataminerConditionKind.PowersAtMost => $"拥有至多 {condition.Amount} 个能力",
            DataminerConditionKind.LostHealthAtLeast => $"本场战斗至少失去过 {condition.Amount} 次生命",
            DataminerConditionKind.PlayedEtherealAtLeast => $"本场战斗至少打出过 {condition.Amount} 张虚无牌",
            DataminerConditionKind.EnemyDebuffsAtLeast => $"敌人身上至少有 {condition.Amount} 个负面效果",
            DataminerConditionKind.PotionsAtLeast => $"拥有至少 {condition.Amount} 瓶药水",
            DataminerConditionKind.GoldAtLeast => $"金币不少于 {condition.Amount}",
            _ => "未知条件"
        };
    }

    public static string BuildAbilityTrigger(DataminerAbilityTriggerKind trigger) => trigger switch
    {
        DataminerAbilityTriggerKind.None => "获得能力后立即触发",
        DataminerAbilityTriggerKind.OnLoseHealth => "每当你失去生命时",
        DataminerAbilityTriggerKind.OnExhaustCard => "每当有一张牌被消耗时",
        DataminerAbilityTriggerKind.OnGiveVulnerable => "每当你给予易伤时",
        DataminerAbilityTriggerKind.OnTurnEnd => "回合结束时",
        DataminerAbilityTriggerKind.OnTurnStart => "回合开始时",
        DataminerAbilityTriggerKind.OnCombatEnd => "战斗结束时",
        DataminerAbilityTriggerKind.OnDrawStrike => "每当你抽到一张打击牌时",
        DataminerAbilityTriggerKind.OnGainBlock => "每当你获得格挡时",
        DataminerAbilityTriggerKind.OnDrawCard => "每当你抽到一张牌时",
        DataminerAbilityTriggerKind.OnPlayCard => "每当你打出一张牌时",
        DataminerAbilityTriggerKind.OnPlaySkill => "每当你打出一张技能牌时",
        DataminerAbilityTriggerKind.OnPlayAttack => "每当你打出一张攻击牌时",
        DataminerAbilityTriggerKind.OnPlayPower => "每当你打出一张能力牌时",
        DataminerAbilityTriggerKind.OnPlayStatus => "每当你打出一张状态牌时",
        DataminerAbilityTriggerKind.OnPlayCurse => "每当你打出一张诅咒牌时",
        DataminerAbilityTriggerKind.OnUnblockedDamage => "每当攻击造成未被格挡的伤害时",
        DataminerAbilityTriggerKind.OnGenerateCard => "每当你生成一张牌时",
        DataminerAbilityTriggerKind.OnSpendOrGainStars => "每当你花费或获得星星时",
        DataminerAbilityTriggerKind.OnSpendEnergy => "每当你花费费用时",
        DataminerAbilityTriggerKind.OnAttack => "每当你攻击敌人时",
        DataminerAbilityTriggerKind.OnGiveEnemyDebuff => "每当你给予敌人负面状态时",
        DataminerAbilityTriggerKind.OnPlaySoul => "每当你打出一张灵魂时",
        DataminerAbilityTriggerKind.OnDrawEthereal => "每当你抽到一张虚无牌时",
        DataminerAbilityTriggerKind.OnPlayEthereal => "每当你打出一张虚无牌时",
        DataminerAbilityTriggerKind.OnDrawStatus => "每当你抽到一张状态牌时",
        DataminerAbilityTriggerKind.OnChannelLightning => "每当你激发闪电充能球时",
        DataminerAbilityTriggerKind.OnGenerateStatus => "每当你生成一张状态牌时",
        DataminerAbilityTriggerKind.OnShuffleDrawPile => "每当你的抽牌堆洗牌时",
        DataminerAbilityTriggerKind.EveryDrawCount => "每抽若干张牌时",
        _ => "错误"
    };

    public static string BuildSubEffect(DataminerSubEffect effect)
    {
        string target = BuildTarget(effect.PowerTarget);
        string result = effect.Kind switch
        {
            DataminerEffectKind.None => "无效果。",
            DataminerEffectKind.Damage => $"造成 {effect.Amount} 点伤害。",
            DataminerEffectKind.DamageAll => $"对所有敌人造成 {effect.Amount} 点伤害。",
            DataminerEffectKind.Block => $"获得 {effect.Amount} 点格挡。",
            DataminerEffectKind.AllyBlock => $"使{target}获得 {effect.Amount} 点格挡。",
            DataminerEffectKind.Draw => $"抽 {effect.Amount} 张牌。",
            DataminerEffectKind.Energy => effect.Amount >= 0
                ? $"获得 {effect.Amount} 点能量。"
                : $"失去 {-effect.Amount} 点能量。",
            DataminerEffectKind.Health => effect.Amount >= 0
                ? $"恢复 {effect.Amount} 点生命。"
                : $"失去 {-effect.Amount} 点生命。",
            DataminerEffectKind.HealthLossByPileCount => "失去你当前所有牌堆数量的生命值。",
            DataminerEffectKind.GenerateShivs => $"将 {effect.Amount} 张小刀加入手牌。",
            DataminerEffectKind.GenerateSouls => $"将 {effect.Amount} 张灵魂加入手牌。",
            DataminerEffectKind.GenerateSoulsToPile => $"将 {effect.Amount} 张灵魂加入{BuildPile(effect.PileScope)}。",
            DataminerEffectKind.RetrieveDiscardCards => $"从弃牌堆选择 {effect.Amount} 张牌放入{BuildPile(effect.PileScope)}。",
            DataminerEffectKind.RetrieveDrawCards => $"从抽牌堆选择 {effect.Amount} 张牌放入{BuildPile(effect.PileScope)}。",
            DataminerEffectKind.ExhaustRandomHand => $"随机消耗 {effect.Amount} 张手牌。",
            DataminerEffectKind.EnemyBlock => $"使敌人获得 {effect.Amount} 点格挡。",
            DataminerEffectKind.RandomPower => $"使{target}获得 {effect.Amount} 层随机能力。",
            DataminerEffectKind.SpecificPower => $"使{target}获得 {effect.Amount} 层特定能力。",
            DataminerEffectKind.RandomBuff => $"使{target}获得 {effect.Amount} 层随机官方能力。",
            DataminerEffectKind.GenerateCards => BuildGeneratedCards(effect),
            DataminerEffectKind.Gold => effect.Amount >= 0
                ? $"获得 {effect.Amount} 金币。"
                : $"失去 {-effect.Amount} 金币。",
            DataminerEffectKind.Potions => $"获得 {effect.Amount} 瓶随机药水。",
            DataminerEffectKind.DamagePerPile => $"每有一张{BuildPile(effect.PileScope)}，造成 {effect.Amount} 点伤害。",
            DataminerEffectKind.ChannelOrb => $"生成 {effect.Amount} 个{BuildOrb(effect.PowerId)}。",
            DataminerEffectKind.Combo => $"增加 {effect.Amount} 次连击。",
            DataminerEffectKind.DiscardAllHand => "丢弃所有手牌。",
            DataminerEffectKind.ExhaustAllHand => "消耗所有手牌。",
            DataminerEffectKind.ExhaustSelectedHand => $"选择至多 {effect.Amount} 张手牌消耗。",
            DataminerEffectKind.AutoPlayPile => $"打出{BuildPile(effect.PileScope)}中的 {effect.Amount} 张牌。",
            DataminerEffectKind.Poison => $"使敌人获得 {effect.Amount} 层中毒。",
            DataminerEffectKind.UpgradePile => $"升级{BuildPile(effect.PileScope)}中的 {effect.Amount} 张牌。",
            DataminerEffectKind.UpgradeAll => "升级所有牌。",
            DataminerEffectKind.Summon => $"召唤 {effect.Amount} 只随机怪物。",
            DataminerEffectKind.Stars => $"获得 {effect.Amount} 颗星星。",
            DataminerEffectKind.EnchantPile => $"使{BuildPile(effect.PileScope)}中的 {effect.Amount} 张牌获得随机附魔。",
            DataminerEffectKind.RandomizeHandCost => "随机化手牌的费用。",
            DataminerEffectKind.DirectWin => "直接赢得战斗。",
            DataminerEffectKind.MaxHp => effect.Amount >= 0
                ? $"使{target}的生命上限增加 {effect.Amount}。"
                : $"使{target}的生命上限减少 {-effect.Amount}。",
            DataminerEffectKind.ExtraCardRewards => $"战斗结束后获得 {effect.Amount} 组卡牌奖励。",
            DataminerEffectKind.ObtainRelic => "获得一件随机遗物。",
            DataminerEffectKind.LoseRelic => "失去一件随机遗物。",
            DataminerEffectKind.DoublePowers => $"使{target}的所有能力效果翻倍。",
            DataminerEffectKind.OstyDamage => $"造成奥斯提当前生命值 {effect.Amount} 倍的伤害。",
            DataminerEffectKind.InstantDeath => "立刻死亡。",
            DataminerEffectKind.MoriyaDance => "播放随机守矢大舞。",
            DataminerEffectKind.SpawnFumo => "召唤随机 Fumo。",
            DataminerEffectKind.ReplaceDeckWithIronWaves => $"删除卡组中的所有卡，然后加入 {effect.Amount} 张铁斩波。",
            DataminerEffectKind.GodMode => effect.Amount == 0
                ? "本回合开启上帝模式。"
                : "本场战斗开启上帝模式。",
            DataminerEffectKind.ClearAllPowers => $"清除{target}身上的全部能力。",
            DataminerEffectKind.ClearEnemyDebuffs => $"清除{target}身上的全部负面效果。",
            DataminerEffectKind.ClearEnemyBuffs => $"清除{target}身上的全部正面效果。",
            DataminerEffectKind.OverlayPlayers => $"将随机卡图覆盖在{target}身上。",
            DataminerEffectKind.OverlayEnemies => $"将随机卡图覆盖在{target}身上。",
            DataminerEffectKind.RandomVfx => $"释放 {effect.Amount} 个随机特效。",
            DataminerEffectKind.SpecificVfx => $"释放 {effect.Amount} 个指定特效。",
            DataminerEffectKind.ModifyHandLimit => effect.Amount >= 0
                ? $"手牌上限增加 {effect.Amount}。"
                : $"手牌上限减少 {-effect.Amount}。",
            DataminerEffectKind.ApplyAllDebuffs => $"使{target}获得所有负面效果各 {effect.Amount} 层。",
            DataminerEffectKind.RepeatPrimary => $"额外执行主效果 {effect.Amount} 次。",
            _ => "未知效果。"
        };

        return result;
    }

    private static string BuildGeneratedCards(DataminerSubEffect effect)
    {
        string kind = effect.GeneratedCardKind == DataminerGeneratedCardKind.SpecificDataminer
            ? "数据破解"
            : "随机";
        return $"生成 {effect.Amount} 张{kind}牌到{BuildDestination(effect.GeneratedCardDestination)}。";
    }

    private static string BuildTarget(DataminerPowerTarget target) => target switch
    {
        DataminerPowerTarget.Self => "自己",
        DataminerPowerTarget.Ally => "一名队友",
        DataminerPowerTarget.AllAllies => "所有队友",
        DataminerPowerTarget.Enemy => "一名敌人",
        DataminerPowerTarget.AllEnemies => "所有敌人",
        DataminerPowerTarget.AllUnits => "所有单位",
        _ => "目标"
    };

    private static string BuildPile(DataminerPileScope? pile) => pile switch
    {
        DataminerPileScope.Deck => "卡组",
        DataminerPileScope.Exhaust => "消耗牌堆",
        DataminerPileScope.Draw => "抽牌堆",
        DataminerPileScope.Discard => "弃牌堆",
        DataminerPileScope.Hand => "手牌",
        DataminerPileScope.All => "所有牌堆",
        _ => "牌堆"
    };

    private static string BuildDestination(DataminerGeneratedCardDestination? destination) => destination switch
    {
        DataminerGeneratedCardDestination.Hand => "手牌",
        DataminerGeneratedCardDestination.Draw => "抽牌堆",
        DataminerGeneratedCardDestination.Discard => "弃牌堆",
        DataminerGeneratedCardDestination.Exhaust => "消耗牌堆",
        DataminerGeneratedCardDestination.Deck => "卡组",
        _ => "手牌"
    };

    private static string BuildOrb(string? orb) => orb?.ToLowerInvariant() switch
    {
        "lightning" => "闪电充能球",
        "glass" => "玻璃充能球",
        "dark" => "黑暗充能球",
        "frost" => "冰霜充能球",
        _ => "充能球"
    };
}
