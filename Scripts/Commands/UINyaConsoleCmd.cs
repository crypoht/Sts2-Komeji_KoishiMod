using System;
using System.Collections.Generic;
using System.Linq;
using KomeijiKoishi.Cards.Fumo;
using KomeijiKoishi.Vfx;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.DevConsole;
using MegaCrit.Sts2.Core.DevConsole.ConsoleCommands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Nodes.Rooms;

namespace KomeijiKoishi.Commands;

public sealed class UINyaConsoleCmd : AbstractConsoleCmd
{
    private static readonly string[] TestModes = { "test" };
    private static readonly string[] EffectNames = { "giftfumo" };
    private static readonly string[] FumoNames = { "reimu", "cirno", "kogasa", "marisa", "okina", "reisen", "tewi", "yukari", "yuuka", "flandre", "lwkoishi", "shion", "youmu", "nue", "yuyuko" };

    public override string CmdName => "uinya";
    public override string Args => "test <effect-name> [fumo-name] [target-index]";
    public override string Description => "UINya mod debug helpers. Example: UINya test giftfumo reimu 1";
    public override bool IsNetworked => false;

    public override CmdResult Process(Player? issuingPlayer, string[] args)
    {
        if (args.Length < 2)
        {
            return new CmdResult(false, "Usage: UINya test <effect-name> [fumo-name]\nEffects: " + string.Join(", ", EffectNames) + "\nFumos: " + string.Join(", ", FumoNames));
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
            List<string> candidates = new(FumoNames);
            if (player?.Creature.CombatState is CombatState combatState)
            {
                for (int i = 0; i < combatState.Creatures.Count; i++)
                {
                    candidates.Add(i.ToString());
                }
            }

            return CompleteArgument(candidates, new[] { args[0], args[1] }, args[2], CompletionType.Argument);
        }

        if (args.Length == 4 && TestModes.Contains(Normalize(args[0])) && EffectNames.Contains(Normalize(args[1])) && !int.TryParse(args[2], out _))
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

        CardModel? fumoCard = ResolveFumoCard(fumoName);
        if (fumoCard == null)
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

        NGiftYouFumoVfx? vfx = NGiftYouFumoVfx.Create(issuingPlayer.Creature, targetCreature, fumoCard);
        if (vfx == null)
        {
            return new CmdResult(false, "Failed to create NGiftYouFumoVfx. Check the log for missing nodes or textures.");
        }

        room.CombatVfxContainer.AddChildSafely(vfx);
        string targetText = targetIndex.HasValue ? $"target #{targetIndex.Value}" : "self";
        return new CmdResult(true, $"Spawned NGiftYouFumoVfx with {(fumoName == null ? "reimu" : Normalize(fumoName))} toward {targetText}.");
    }

    private static CardModel? ResolveFumoCard(string? fumoName)
    {
        string name = string.IsNullOrWhiteSpace(fumoName) ? "reimu" : Normalize(fumoName);
        return name switch
        {
            "reimu" or "reimufumo" => ModelDb.Card<ReimuFumo_Koishi>(),
            "cirno" or "cirnofumo" => ModelDb.Card<CirnoFumo_Koishi>(),
            "kogasa" or "kogasafumo" => ModelDb.Card<KogasaFumo_Koishi>(),
            "marisa" or "marisafumo" => ModelDb.Card<MarisaFumo_Koishi>(),
            "okina" or "okinafumo" => ModelDb.Card<OkinaFumo_Koishi>(),
            "reisen" or "reisenfumo" => ModelDb.Card<ReisenFumo_Koishi>(),
            "tewi" or "tewifumo" => ModelDb.Card<TewiFumo_Koishi>(),
            "yukari" or "yukarifumo" => ModelDb.Card<YukariFumo_Koishi>(),
            "yuuka" or "yuukafumo" => ModelDb.Card<YuukaFumo_Koishi>(),
            "flandre" or "flandrefumo" => ModelDb.Card<FlandreFumo_Koishi>(),
            "lwkoishi" or "lwkoishifumo" => ModelDb.Card<LWKoishiFumo_Koishi>(),
            "shion" or "shionfumo" => ModelDb.Card<ShionFumo_Koishi>(),
            "youmu" or "youmufumo" => ModelDb.Card<YoumuFumo_Koishi>(),
            "nue" or "nuefumo" => ModelDb.Card<NueFumo_Koishi>(),
            "yuyuko" or "yuyukofumo" => ModelDb.Card<YuyukoFumo_Koishi>(),
            _ => null
        };
    }

    private static string Normalize(string value)
    {
        return value.Trim().Replace("_", string.Empty).Replace("-", string.Empty).ToLowerInvariant();
    }
}
