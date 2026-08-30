using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using KomeijiKoishi.Cards;
using KomeijiKoishi.Cards.Fumo;
using KomeijiKoishi.Dataminer;
using KomeijiKoishi.Patches;
using KomeijiKoishi.Vfx;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.DevConsole;
using MegaCrit.Sts2.Core.DevConsole.ConsoleCommands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Nodes.Rooms;

namespace KomeijiKoishi.Commands;

public sealed class UINyaConsoleCmd : AbstractConsoleCmd
{
    private static readonly string[] TestModes = { "test" };
    private static readonly string[] EffectNames = { "giftfumo", "errorcard", "seeerror" };
    private static readonly string[] FumoNames = NGiftYouFumoVfx.TextureNames.ToArray();
    private static readonly IReadOnlyDictionary<string, string> FumoLabels = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
    {
        ["cirno"] = "琪露诺",
        ["clownpiece"] = "克劳恩皮丝",
        ["daiyousei"] = "大妖精",
        ["flandre"] = "芙兰朵露",
        ["kogasa"] = "小伞",
        ["junko"] = "纯狐",
        ["keiki"] = "埴安神袿姬",
        ["koakuma"] = "小恶魔",
        ["lizunamaru"] = "饭纲丸龙",
        ["lwkoishi"] = "白恋",
        ["maribel"] = "梅莉",
        ["marisa"] = "魔理沙",
        ["marisamoon"] = "月战魔理沙",
        ["marisapast"] = "反色旧作魔理沙",
        ["minamitu"] = "村纱水蜜",
        ["nue"] = "正体不明",
        ["okina"] = "摩多罗",
        ["pinkkoishi"] = "粉恋",
        ["reimu"] = "灵梦",
        ["reimumoon"] = "月战灵梦",
        ["reimupast"] = "反色旧作灵梦",
        ["reisen"] = "铃仙",
        ["renko"] = "莲子",
        ["sanae"] = "早苗",
        ["sastori"] = "觉",
        ["satori"] = "觉",
        ["shion"] = "紫苑",
        ["suwako"] = "诹访子",
        ["tewi"] = "帝",
        ["youmu"] = "妖梦",
        ["yukari"] = "八云紫",
        ["yuuka"] = "幽香",
        ["yuyuko"] = "幽幽子"
    };
    private static readonly IReadOnlyDictionary<string, DataminerConditionKind> ErrorCardConditions =
        new Dictionary<string, DataminerConditionKind>(StringComparer.OrdinalIgnoreCase)
        {
            ["null"] = DataminerConditionKind.None,
            ["handmin"] = DataminerConditionKind.HandAtLeast,
            ["handmax"] = DataminerConditionKind.HandAtMost,
            ["drawempty"] = DataminerConditionKind.DrawPileEmpty,
            ["discardempty"] = DataminerConditionKind.DiscardPileEmpty,
            ["enemymin"] = DataminerConditionKind.EnemiesAtLeast,
            ["allymin"] = DataminerConditionKind.AlliesAtLeast,
            ["exhaustmin"] = DataminerConditionKind.ExhaustPileAtLeast,
            ["attackmin"] = DataminerConditionKind.PlayedAttacksAtLeast,
            ["skillmin"] = DataminerConditionKind.PlayedSkillsAtLeast,
            ["powermin"] = DataminerConditionKind.PowersAtLeast,
            ["powermax"] = DataminerConditionKind.PowersAtMost,
            ["losthp"] = DataminerConditionKind.LostHealthAtLeast,
            ["etherealmin"] = DataminerConditionKind.PlayedEtherealAtLeast,
            ["enemydebuffmin"] = DataminerConditionKind.EnemyDebuffsAtLeast,
            ["potionmin"] = DataminerConditionKind.PotionsAtLeast,
            ["goldmin"] = DataminerConditionKind.GoldAtLeast
        };
    private static readonly IReadOnlyDictionary<string, string> ErrorCardConditionLabels =
        new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            ["null"] = "无条件",
            ["handmin"] = "手牌不少于",
            ["handmax"] = "手牌不多于",
            ["drawempty"] = "抽牌堆为空",
            ["discardempty"] = "弃牌堆为空",
            ["enemymin"] = "敌人不少于",
            ["allymin"] = "队友不少于",
            ["exhaustmin"] = "消耗堆不少于",
            ["attackmin"] = "已打出攻击牌不少于",
            ["skillmin"] = "已打出技能牌不少于",
            ["powermin"] = "能力不少于",
            ["powermax"] = "能力不多于",
            ["losthp"] = "失去生命次数不少于",
            ["etherealmin"] = "已打出虚无牌不少于",
            ["enemydebuffmin"] = "敌人负面效果不少于",
            ["potionmin"] = "药水不少于",
            ["goldmin"] = "金币不少于"
        };
    private static readonly IReadOnlyDictionary<string, DataminerEffectKind> ErrorCardEffects =
        new Dictionary<string, DataminerEffectKind>(StringComparer.OrdinalIgnoreCase)
        {
            ["null"] = DataminerEffectKind.None,
            ["damage"] = DataminerEffectKind.Damage,
            ["aoe"] = DataminerEffectKind.DamageAll,
            ["block"] = DataminerEffectKind.Block,
            ["allyblock"] = DataminerEffectKind.AllyBlock,
            ["draw"] = DataminerEffectKind.Draw,
            ["energy"] = DataminerEffectKind.Energy,
            ["health"] = DataminerEffectKind.Health,
            ["exhaustrandom"] = DataminerEffectKind.ExhaustRandomHand,
            ["enemyblock"] = DataminerEffectKind.EnemyBlock,
            ["power"] = DataminerEffectKind.RandomPower,
            ["fixedpower"] = DataminerEffectKind.SpecificPower,
            ["buff"] = DataminerEffectKind.RandomBuff,
            ["cards"] = DataminerEffectKind.GenerateCards,
            ["gold"] = DataminerEffectKind.Gold,
            ["potions"] = DataminerEffectKind.Potions,
            ["piledamage"] = DataminerEffectKind.DamagePerPile,
            ["orb"] = DataminerEffectKind.ChannelOrb,
            ["combo"] = DataminerEffectKind.Combo,
            ["discardall"] = DataminerEffectKind.DiscardAllHand,
            ["exhaustall"] = DataminerEffectKind.ExhaustAllHand,
            ["exhaustchoose"] = DataminerEffectKind.ExhaustSelectedHand,
            ["autoplay"] = DataminerEffectKind.AutoPlayPile,
            ["poison"] = DataminerEffectKind.Poison,
            ["upgrade"] = DataminerEffectKind.UpgradePile,
            ["upgradeall"] = DataminerEffectKind.UpgradeAll,
            ["summon"] = DataminerEffectKind.Summon,
            ["stars"] = DataminerEffectKind.Stars,
            ["enchant"] = DataminerEffectKind.EnchantPile,
            ["randomcost"] = DataminerEffectKind.RandomizeHandCost,
            ["win"] = DataminerEffectKind.DirectWin,
            ["maxhp"] = DataminerEffectKind.MaxHp,
            ["rewards"] = DataminerEffectKind.ExtraCardRewards,
            ["relic"] = DataminerEffectKind.ObtainRelic,
            ["loserelic"] = DataminerEffectKind.LoseRelic,
            ["doublepowers"] = DataminerEffectKind.DoublePowers,
            ["osty"] = DataminerEffectKind.OstyDamage,
            ["die"] = DataminerEffectKind.InstantDeath,
            ["dance"] = DataminerEffectKind.MoriyaDance,
            ["fumo"] = DataminerEffectKind.SpawnFumo,
            ["ironwave"] = DataminerEffectKind.ReplaceDeckWithIronWaves,
            ["godmode"] = DataminerEffectKind.GodMode,
            ["clearpowers"] = DataminerEffectKind.ClearAllPowers,
            ["cleardebuff"] = DataminerEffectKind.ClearEnemyDebuffs,
            ["clearbuff"] = DataminerEffectKind.ClearEnemyBuffs,
            ["overlayplayer"] = DataminerEffectKind.OverlayPlayers,
            ["overlayenemy"] = DataminerEffectKind.OverlayEnemies,
            ["vfx"] = DataminerEffectKind.RandomVfx,
            ["fixedvfx"] = DataminerEffectKind.SpecificVfx,
            ["handlimit"] = DataminerEffectKind.ModifyHandLimit,
            ["alldebuffs"] = DataminerEffectKind.ApplyAllDebuffs,
            ["pilehp"] = DataminerEffectKind.HealthLossByPileCount,
            ["shiv"] = DataminerEffectKind.GenerateShivs,
            ["soul"] = DataminerEffectKind.GenerateSouls,
            ["soulpile"] = DataminerEffectKind.GenerateSoulsToPile,
            ["retrievediscard"] = DataminerEffectKind.RetrieveDiscardCards,
            ["retrievedraw"] = DataminerEffectKind.RetrieveDrawCards,
            ["repeat"] = DataminerEffectKind.RepeatPrimary
        };
    private static readonly IReadOnlyDictionary<string, string> ErrorCardEffectLabels =
        new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            ["null"] = "无效果",
            ["damage"] = "单体伤害",
            ["aoe"] = "全体伤害",
            ["block"] = "获得格挡",
            ["allyblock"] = "队友获得格挡",
            ["draw"] = "抽牌",
            ["energy"] = "获得或失去能量",
            ["health"] = "获得或失去生命",
            ["exhaustrandom"] = "随机消耗手牌",
            ["enemyblock"] = "敌人获得格挡",
            ["power"] = "获得随机能力",
            ["fixedpower"] = "获得固定能力",
            ["buff"] = "获得随机官方能力",
            ["cards"] = "生成卡牌",
            ["gold"] = "获得或失去金币",
            ["potions"] = "获得随机药水",
            ["piledamage"] = "按牌堆数量造成伤害",
            ["orb"] = "生成充能球",
            ["combo"] = "增加连击",
            ["discardall"] = "丢弃所有手牌",
            ["exhaustall"] = "消耗所有手牌",
            ["exhaustchoose"] = "选择消耗手牌",
            ["autoplay"] = "自动打出牌堆卡牌",
            ["poison"] = "施加中毒",
            ["upgrade"] = "升级牌堆卡牌",
            ["upgradeall"] = "升级所有牌",
            ["summon"] = "召唤怪物",
            ["stars"] = "获得星星",
            ["enchant"] = "给牌堆附魔",
            ["randomcost"] = "随机化手牌费用",
            ["win"] = "直接胜利",
            ["maxhp"] = "改变生命上限",
            ["rewards"] = "额外卡牌奖励",
            ["relic"] = "获得遗物",
            ["loserelic"] = "失去遗物",
            ["doublepowers"] = "翻倍能力",
            ["osty"] = "奥斯提伤害",
            ["die"] = "立刻死亡",
            ["dance"] = "播放守矢大舞",
            ["fumo"] = "召唤 Fumo",
            ["ironwave"] = "牌组变为铁斩波",
            ["godmode"] = "开启上帝模式",
            ["clearpowers"] = "清除能力",
            ["cleardebuff"] = "清除敌人负面效果",
            ["clearbuff"] = "清除敌人正面效果",
            ["overlayplayer"] = "覆盖玩家模型",
            ["overlayenemy"] = "覆盖怪物模型",
            ["vfx"] = "播放随机特效",
            ["fixedvfx"] = "播放固定特效",
            ["handlimit"] = "改变手牌上限",
            ["alldebuffs"] = "施加所有负面效果",
            ["repeat"] = "额外执行主效果"
        };
    private static readonly IReadOnlyDictionary<string, CardType?> ErrorCardTypes =
        new Dictionary<string, CardType?>(StringComparer.OrdinalIgnoreCase)
        {
            ["random"] = null,
            ["attack"] = CardType.Attack,
            ["skill"] = CardType.Skill,
            ["power"] = CardType.Power,
            ["status"] = CardType.Status,
            ["curse"] = CardType.Curse
        };
    private static readonly IReadOnlyDictionary<string, string> ErrorCardTypeLabels =
        new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            ["random"] = "随机",
            ["attack"] = "攻击",
            ["skill"] = "技能",
            ["power"] = "能力",
            ["status"] = "状态",
            ["curse"] = "诅咒"
        };
    private static readonly IReadOnlyDictionary<string, DataminerReturnPile> ErrorCardReturns =
        new Dictionary<string, DataminerReturnPile>(StringComparer.OrdinalIgnoreCase)
        {
            ["null"] = DataminerReturnPile.None,
            ["drawtop"] = DataminerReturnPile.DrawTop,
            ["drawbottom"] = DataminerReturnPile.DrawBottom,
            ["hand"] = DataminerReturnPile.Hand,
            ["exhaust"] = DataminerReturnPile.Exhaust
        };
    private static readonly IReadOnlyDictionary<string, string> ErrorCardReturnLabels =
        new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            ["null"] = "无回收效果",
            ["drawtop"] = "回到抽牌堆顶",
            ["drawbottom"] = "回到抽牌堆底",
            ["hand"] = "回到手牌",
            ["exhaust"] = "回到消耗牌堆"
        };
    private static readonly IReadOnlyDictionary<string, DataminerAbilityTriggerKind> ErrorCardAbilityTriggers =
        Enum.GetValues<DataminerAbilityTriggerKind>()
            .ToDictionary(
                trigger => trigger switch
                {
                    DataminerAbilityTriggerKind.None => "null",
                    DataminerAbilityTriggerKind.OnLoseHealth => "losehp",
                    DataminerAbilityTriggerKind.OnExhaustCard => "exhaust",
                    DataminerAbilityTriggerKind.OnTurnEnd => "turnend",
                    DataminerAbilityTriggerKind.OnTurnStart => "turnstart",
                    DataminerAbilityTriggerKind.OnCombatEnd => "combatend",
                    DataminerAbilityTriggerKind.OnDrawCard => "draw",
                    DataminerAbilityTriggerKind.OnPlayCard => "play",
                    DataminerAbilityTriggerKind.OnPlaySkill => "playskill",
                    DataminerAbilityTriggerKind.OnPlayAttack => "playattack",
                    DataminerAbilityTriggerKind.OnPlayPower => "playpower",
                    DataminerAbilityTriggerKind.OnPlayStatus => "playstatus",
                    DataminerAbilityTriggerKind.OnPlayCurse => "playcurse",
                    DataminerAbilityTriggerKind.OnUnblockedDamage => "unblockeddamage",
                    DataminerAbilityTriggerKind.OnAttack => "attack",
                    DataminerAbilityTriggerKind.OnPlaySoul => "playsoul",
                    DataminerAbilityTriggerKind.OnDrawEthereal => "drawethereal",
                    DataminerAbilityTriggerKind.OnPlayEthereal => "playethereal",
                    DataminerAbilityTriggerKind.OnDrawStatus => "drawstatus",
                    DataminerAbilityTriggerKind.OnGenerateCard => "generate",
                    DataminerAbilityTriggerKind.OnGenerateStatus => "generatestatus",
                    DataminerAbilityTriggerKind.OnShuffleDrawPile => "shuffle",
                    DataminerAbilityTriggerKind.EveryDrawCount => "everydraw",
                    _ => trigger.ToString().ToLowerInvariant()
                },
                trigger => trigger);
    private static readonly IReadOnlyDictionary<string, string> ErrorCardAbilityTriggerLabels =
        new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            ["null"] = "获得后立即触发",
            ["losehp"] = "失去生命时",
            ["exhaust"] = "消耗牌时",
            ["turnend"] = "回合结束时",
            ["turnstart"] = "回合开始时",
            ["combatend"] = "战斗结束时",
            ["draw"] = "抽牌时",
            ["play"] = "打出牌时",
            ["playskill"] = "打出技能牌时",
            ["playattack"] = "打出攻击牌时",
            ["playpower"] = "打出能力牌时",
            ["playstatus"] = "打出状态牌时",
            ["playcurse"] = "打出诅咒牌时",
            ["unblockeddamage"] = "造成未被格挡伤害时",
            ["attack"] = "攻击敌人时",
            ["playsoul"] = "打出灵魂时",
            ["drawethereal"] = "抽到虚无牌时",
            ["playethereal"] = "打出虚无牌时",
            ["drawstatus"] = "抽到状态牌时",
            ["generate"] = "生成牌时",
            ["generatestatus"] = "生成状态牌时",
            ["shuffle"] = "抽牌堆洗牌时",
            ["everydraw"] = "每抽若干张牌时"
        };
    private static readonly IReadOnlyDictionary<string, bool> ErrorCardAutoPlay =
        new Dictionary<string, bool>(StringComparer.OrdinalIgnoreCase)
        {
            ["false"] = false,
            ["true"] = true
        };
    private static readonly IReadOnlyDictionary<string, string> ErrorCardAutoPlayLabels =
        new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            ["false"] = "不自动打出",
            ["true"] = "回合结束时自动打出"
        };
    private static readonly PileType[] ErrorCardPiles =
    {
        PileType.Hand,
        PileType.Draw,
        PileType.Discard,
        PileType.Exhaust
    };

    public override string CmdName => "uinya";
    public override string Args => "test giftfumo [fumo-name] [target-index] | test errorcard <type> <condition/trigger> <effect> <extra-condition> <extra-effect> [cost] [return] [auto] [pile] | test seeerror <true|false>";
    public override string Description => "UINya mod debug helpers.";
    public override bool IsNetworked => false;

    public override CmdResult Process(Player? issuingPlayer, string[] args)
    {
        if (args.Length < 2)
        {
            return new CmdResult(false, "Usage: uinya test giftfumo [fumo-name] [target-index] | uinya test errorcard <type> <condition/trigger> <effect> <extra-condition> <extra-effect> [cost] [return] [auto] [pile]");
        }

        string mode = Normalize(args[0]);
        if (!TestModes.Contains(mode))
        {
            return new CmdResult(false, $"Unknown UINya mode '{args[0]}'. Use: UINya test <effect-name>");
        }

        string effect = Normalize(args[1]);
        return effect switch
        {
            "giftfumo" => TestGiftYouFumoVfx(issuingPlayer, args.Skip(2).ToArray()),
            "errorcard" => TestErrorCard(issuingPlayer, args.Skip(2).ToArray()),
            "seeerror" => TestSeeError(issuingPlayer, args.Skip(2).ToArray()),
            _ => new CmdResult(false, $"Unknown UINya test effect '{args[1]}'. Effects: {string.Join(", ", EffectNames)}")
        };
    }

    public override CompletionResult GetArgumentCompletions(Player? player, string[] args)
    {
        if (args.Length <= 1)
        {
            return CompleteArgument(TestModes, Array.Empty<string>(), args.FirstOrDefault() ?? string.Empty, CompletionType.Subcommand);
        }

        if (args.Length == 2 && TestModes.Contains(Normalize(args[0])))
        {
            return CompleteArgument(EffectNames, new[] { args[0] }, args[1], CompletionType.Argument);
        }

        if (args.Length == 3 && TestModes.Contains(Normalize(args[0])) && EffectNames.Contains(Normalize(args[1])))
        {
            if (Normalize(args[1]) == "seeerror")
            {
                return CompleteArgument(
                    new[] { "true", "false" },
                    new[] { args[0], args[1] },
                    args[2],
                    CompletionType.Argument);
            }

            if (Normalize(args[1]) == "errorcard")
            {
                return CompleteArgument(
                    FormatCandidates(ErrorCardTypes.Keys, ErrorCardTypeLabels),
                    new[] { args[0], args[1] },
                    args[2],
                    CompletionType.Argument);
            }

            List<string> candidates = FormatCandidates(FumoNames, FumoLabels).ToList();
            if (player?.Creature.CombatState is CombatState combatState)
            {
                for (int i = 0; i < combatState.Creatures.Count; i++)
                {
                    candidates.Add(i.ToString());
                }
            }

            return CompleteArgument(candidates, new[] { args[0], args[1] }, args[2], CompletionType.Argument);
        }

        if (args.Length == 4 && TestModes.Contains(Normalize(args[0])) && Normalize(args[1]) == "errorcard")
        {
            if (Normalize(args[2]) == "power")
            {
                return CompleteArgument(
                    FormatCandidates(ErrorCardAbilityTriggers.Keys, ErrorCardAbilityTriggerLabels),
                    new[] { args[0], args[1], args[2] },
                    args[3],
                    CompletionType.Argument);
            }

            return CompleteArgument(
                FormatCandidates(ErrorCardConditions.Keys, ErrorCardConditionLabels),
                new[] { args[0], args[1], args[2] },
                args[3],
                CompletionType.Argument);
        }

        if (args.Length == 5 && TestModes.Contains(Normalize(args[0])) && Normalize(args[1]) == "errorcard")
        {
            if (Normalize(args[2]) == "power")
            {
                return CompleteArgument(
                    FormatCandidates(
                        ErrorCardEffects.Keys.Where(key => !key.Equals("repeat", StringComparison.OrdinalIgnoreCase)),
                        ErrorCardEffectLabels),
                    new[] { args[0], args[1], args[2], args[3] },
                    args[4],
                    CompletionType.Argument);
            }

            return CompleteArgument(
                FormatCandidates(
                    ErrorCardEffects.Keys.Where(key => !key.Equals("repeat", StringComparison.OrdinalIgnoreCase)),
                    ErrorCardEffectLabels),
                new[] { args[0], args[1], args[2], args[3] },
                args[4],
                CompletionType.Argument);
        }

        if (args.Length == 6 && TestModes.Contains(Normalize(args[0])) && Normalize(args[1]) == "errorcard")
        {
            if (Normalize(args[2]) == "power")
            {
                return CompleteArgument(
                    new[] { "0 (费用)", "1 (费用)", "2 (费用)", "3 (费用)", "4 (费用)" },
                    new[] { args[0], args[1], args[2], args[3], args[4] },
                    args[5],
                    CompletionType.Argument);
            }

            return CompleteArgument(
                FormatCandidates(ErrorCardConditions.Keys, ErrorCardConditionLabels),
                new[] { args[0], args[1], args[2], args[3], args[4] },
                args[5],
                CompletionType.Argument);
        }

        if (args.Length == 7 && TestModes.Contains(Normalize(args[0])) && Normalize(args[1]) == "errorcard")
        {
            if (Normalize(args[2]) == "power")
            {
                return CompleteArgument(
                    FormatCandidates(ErrorCardAutoPlay.Keys, ErrorCardAutoPlayLabels),
                    new[] { args[0], args[1], args[2], args[3], args[4], args[5] },
                    args[6],
                    CompletionType.Argument);
            }

            return CompleteArgument(
                FormatCandidates(ErrorCardEffects.Keys, ErrorCardEffectLabels),
                new[] { args[0], args[1], args[2], args[3], args[4], args[5] },
                args[6],
                CompletionType.Argument);
        }

        if (args.Length == 8 && TestModes.Contains(Normalize(args[0])) && Normalize(args[1]) == "errorcard")
        {
            if (Normalize(args[2]) == "power")
            {
                return CompleteArgument(
                    ErrorCardPiles.Select(pile => pile.ToString().ToLowerInvariant()),
                    new[] { args[0], args[1], args[2], args[3], args[4], args[5], args[6] },
                    args[7],
                    CompletionType.Argument);
            }

            return CompleteArgument(
                new[] { "0 (费用)", "1 (费用)", "2 (费用)", "3 (费用)", "4 (费用)" },
                new[] { args[0], args[1], args[2], args[3], args[4], args[5], args[6] },
                args[7],
                CompletionType.Argument);
        }

        if (args.Length == 9 && TestModes.Contains(Normalize(args[0])) && Normalize(args[1]) == "errorcard")
        {
            if (Normalize(args[2]) == "power")
            {
                return new CompletionResult
                {
                    Type = CompletionType.Argument,
                    ArgumentContext = CmdName
                };
            }

            return CompleteArgument(
                FormatCandidates(ErrorCardReturns.Keys, ErrorCardReturnLabels),
                new[] { args[0], args[1], args[2], args[3], args[4], args[5], args[6], args[7] },
                args[8],
                CompletionType.Argument);
        }

        if (args.Length == 10 && TestModes.Contains(Normalize(args[0])) && Normalize(args[1]) == "errorcard")
        {
            if (Normalize(args[2]) == "power")
            {
                return new CompletionResult
                {
                    Type = CompletionType.Argument,
                    ArgumentContext = CmdName
                };
            }

            return CompleteArgument(
                FormatCandidates(ErrorCardAutoPlay.Keys, ErrorCardAutoPlayLabels),
                new[] { args[0], args[1], args[2], args[3], args[4], args[5], args[6], args[7], args[8] },
                args[9],
                CompletionType.Argument);
        }

        if (args.Length == 11 && TestModes.Contains(Normalize(args[0])) && Normalize(args[1]) == "errorcard")
        {
            return CompleteArgument(
                ErrorCardPiles.Select(pile => pile.ToString().ToLowerInvariant()),
                new[] { args[0], args[1], args[2], args[3], args[4], args[5], args[6], args[7], args[8], args[9] },
                args[10],
                CompletionType.Argument);
        }

        if (args.Length == 4 && TestModes.Contains(Normalize(args[0])) && Normalize(args[1]) == "giftfumo" && !int.TryParse(args[2], out _))
        {
            List<string> candidates = new();
            if (player?.Creature.CombatState is CombatState combatState)
            {
                for (int i = 0; i < combatState.Creatures.Count; i++)
                {
                    candidates.Add(i.ToString());
                }
            }

            return CompleteArgument(candidates, new[] { args[0], args[1], args[2] }, args[3], CompletionType.Argument);
        }

        return new CompletionResult
        {
            Type = CompletionType.Argument,
            ArgumentContext = CmdName
        };
    }

    private static CmdResult TestErrorCard(Player? issuingPlayer, string[] extraArgs)
    {
        if (issuingPlayer == null || issuingPlayer.Creature.CombatState is not CombatState combatState)
        {
            return new CmdResult(false, "uinya test errorcard requires an active combat.");
        }

        if (!TryGetMappedValue(ErrorCardTypes, extraArgs[0], out CardType? cardType))
        {
            return new CmdResult(false, "Unknown card type. Valid types: random, attack, skill, power, status, curse.");
        }

        bool isPower = cardType == CardType.Power;
        DataminerConditionKind playCondition = DataminerConditionKind.None;
        DataminerEffectKind primaryEffect = DataminerEffectKind.None;
        DataminerConditionKind extraCondition = DataminerConditionKind.None;
        DataminerEffectKind extraEffect = DataminerEffectKind.None;
        DataminerAbilityTriggerKind abilityTrigger = DataminerAbilityTriggerKind.None;
        DataminerSubEffect abilitySubEffect = DataminerSubEffect.None;

        if (isPower)
        {
            if (extraArgs.Length is < 3 or > 6
                || !TryGetMappedValue(ErrorCardAbilityTriggers, extraArgs[1], out abilityTrigger)
                || !TryGetMappedValue(ErrorCardEffects, extraArgs[2], out DataminerEffectKind abilityEffectKind)
                || abilityEffectKind == DataminerEffectKind.RepeatPrimary)
            {
                return new CmdResult(false, "Usage: uinya test errorcard power <trigger> <effect> [cost] [auto] [pile]");
            }

            abilitySubEffect = DataminerEffect.CreateAbilityEffectForConsole(
                issuingPlayer,
                abilityEffectKind);
        }
        else
        {
            if (extraArgs.Length is < 5 or > 9
                || !TryGetMappedValue(ErrorCardConditions, extraArgs[1], out playCondition)
                || !TryGetMappedValue(ErrorCardEffects, extraArgs[2], out primaryEffect)
                || primaryEffect == DataminerEffectKind.RepeatPrimary
                || !TryGetMappedValue(ErrorCardConditions, extraArgs[3], out extraCondition)
                || !TryGetMappedValue(ErrorCardEffects, extraArgs[4], out extraEffect))
            {
                return new CmdResult(false, "Usage: uinya test errorcard <type> <condition> <effect> <extra-condition> <extra-effect> [cost] [return] [auto] [pile]");
            }
        }

        int cost = 0;
        int costIndex = isPower ? 3 : 5;
        if (extraArgs.Length > costIndex
            && (!int.TryParse(Normalize(extraArgs[costIndex]), out cost) || cost is < 0 or > 4))
        {
            return new CmdResult(false, "Invalid cost. Valid costs: 0, 1, 2, 3, 4.");
        }

        DataminerReturnPile returnPile = DataminerReturnPile.None;
        int returnIndex = isPower ? -1 : costIndex + 1;
        if (!isPower
            && extraArgs.Length > returnIndex
            && !TryGetMappedValue(ErrorCardReturns, extraArgs[returnIndex], out returnPile))
        {
            return new CmdResult(false, "Unknown return effect. Valid values: null, drawtop, drawbottom, hand, exhaust.");
        }

        bool autoPlay = false;
        int autoIndex = isPower ? costIndex + 1 : returnIndex + 1;
        if (extraArgs.Length > autoIndex && !TryGetMappedValue(ErrorCardAutoPlay, extraArgs[autoIndex], out autoPlay))
        {
            return new CmdResult(false, "Unknown auto-play value. Valid values: false, true.");
        }

        PileType pile = PileType.Hand;
        int pileIndex = autoIndex + 1;
        if (extraArgs.Length > pileIndex
            && (!Enum.TryParse(Normalize(extraArgs[pileIndex]), true, out pile) || !ErrorCardPiles.Contains(pile)))
        {
            return new CmdResult(false, "Unknown pile. Valid piles: hand, draw, discard, exhaust.");
        }

        if (pile == PileType.Hand && PileType.Hand.GetPile(issuingPlayer).Cards.Count >= CardPile.MaxCardsInHand)
        {
            return new CmdResult(false, $"The hand is full ({CardPile.MaxCardsInHand}).");
        }

        DataminerCard_Koishi card = CreateErrorCard(combatState, issuingPlayer);
        card.SetEffect(DataminerEffect.CreateForConsole(
            issuingPlayer,
            playCondition,
            primaryEffect,
            extraCondition,
            extraEffect,
            cost,
            cardType,
            returnPile,
            autoPlay,
            abilityTrigger,
            abilitySubEffect));

        Task addCardTask = CardPileCmd.AddGeneratedCardToCombat(
            card,
            pile,
            issuingPlayer,
            CardPilePosition.Bottom);
        return new CmdResult(addCardTask, true, $"Added an error card to '{pile}'.");
    }

    private static CmdResult TestSeeError(Player? issuingPlayer, string[] extraArgs)
    {
        if (extraArgs.Length != 1 || !bool.TryParse(Normalize(extraArgs[0]), out bool enabled))
        {
            return new CmdResult(false, "Usage: uinya test seeerror <true|false>");
        }

        DataminerDescriptionPatch.SetReadable(enabled, issuingPlayer);
        return new CmdResult(
            true,
            enabled
                ? "Dataminer descriptions are now readable."
                : "Dataminer descriptions are now obfuscated.");
    }

    private static CmdResult TestGiftYouFumoVfx(Player? issuingPlayer, string[] extraArgs)
    {
        if (issuingPlayer == null)
        {
            return new CmdResult(false, "UINya test giftfumo requires an active player.");
        }

        if (issuingPlayer.Creature.CombatState is not CombatState combatState)
        {
            return new CmdResult(false, "UINya test giftfumo requires an active combat.");
        }

        NCombatRoom? room = NCombatRoom.Instance;
        if (room?.CombatVfxContainer == null)
        {
            return new CmdResult(false, "UINya test giftfumo requires an active combat room.");
        }

        string? fumoName = null;
        int? targetIndex = null;
        if (extraArgs.Length >= 1)
        {
            if (extraArgs.Length == 1 && int.TryParse(extraArgs[0], out int onlyIndex))
            {
                targetIndex = onlyIndex;
            }
            else
            {
                fumoName = extraArgs[0];
            }
        }

        if (extraArgs.Length >= 2)
        {
            if (int.TryParse(extraArgs[1], out int parsedIndex))
            {
                targetIndex = parsedIndex;
            }
            else
            {
                return new CmdResult(false, $"Unknown target index '{extraArgs[1]}'.");
            }
        }

        string resolvedFumoName = fumoName == null ? "reimu" : Normalize(fumoName);
        if (!NGiftYouFumoVfx.TryGetTexturePaths(resolvedFumoName, out _, out _))
        {
            return new CmdResult(false, $"Unknown Fumo '{fumoName}'. Fumos: {string.Join(", ", FumoNames)}");
        }

        Creature targetCreature = issuingPlayer.Creature;
        if (targetIndex.HasValue)
        {
            IReadOnlyList<Creature> creatures = combatState.Creatures;
            if (targetIndex.Value < 0 || targetIndex.Value >= creatures.Count)
            {
                return new CmdResult(false, $"Target index '{targetIndex.Value}' is out of range. Valid range: 0..{creatures.Count - 1}");
            }

            targetCreature = creatures[targetIndex.Value];
        }

        if (!targetCreature.IsAlive)
        {
            return new CmdResult(false, $"Target index '{targetIndex?.ToString() ?? "self"}' is not alive.");
        }

        NGiftYouFumoVfx? vfx = NGiftYouFumoVfx.Create(issuingPlayer.Creature, targetCreature, resolvedFumoName);
        if (vfx == null)
        {
            return new CmdResult(false, "Failed to create NGiftYouFumoVfx. Check the log for missing nodes or textures.");
        }

        room.CombatVfxContainer.AddChildSafely(vfx);
        string targetText = targetIndex.HasValue ? $"target #{targetIndex.Value}" : "self";
        return new CmdResult(true, $"Spawned NGiftYouFumoVfx with {resolvedFumoName} toward {targetText}.");
    }

    private static string Normalize(string value)
    {
        string normalized = value.Trim();
        int labelStart = normalized.LastIndexOf(" (", StringComparison.Ordinal);
        if (labelStart >= 0 && normalized.EndsWith(')'))
        {
            normalized = normalized[..labelStart];
        }

        return normalized.Replace("_", string.Empty).Replace("-", string.Empty).ToLowerInvariant();
    }

    private static IEnumerable<string> FormatCandidates(
        IEnumerable<string> candidates,
        IReadOnlyDictionary<string, string> labels)
    {
        return candidates.Select(candidate => labels.TryGetValue(candidate, out string? label)
            ? $"{candidate} ({label})"
            : candidate);
    }

    private static bool TryGetMappedValue<T>(
        IReadOnlyDictionary<string, T> values,
        string input,
        out T result)
    {
        return values.TryGetValue(Normalize(input), out result!);
    }

    private static DataminerCard_Koishi CreateErrorCard(CombatState combatState, Player player)
    {
        return player.RunState.Rng.CombatCardGeneration.NextInt(7) switch
        {
            0 => combatState.CreateCard<DataminerCard_Koishi>(player),
            1 => combatState.CreateCard<DataminerCard_Koishi_2>(player),
            2 => combatState.CreateCard<DataminerCard_Koishi_3>(player),
            3 => combatState.CreateCard<DataminerCard_Koishi_4>(player),
            4 => combatState.CreateCard<DataminerCard_Koishi_5>(player),
            5 => combatState.CreateCard<DataminerCard_Koishi_6>(player),
            _ => combatState.CreateCard<DataminerCard_Koishi_7>(player)
        };
    }
}
