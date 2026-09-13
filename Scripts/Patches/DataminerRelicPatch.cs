using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using Godot;
using HarmonyLib;
using BaseLib.Utils;
using KomeijiKoishi.Cards;
using KomeijiKoishi.Dataminer;
using KomeijiKoishi.Multiplayer;
using KomeijiKoishi.Relics;
using MegaCrit.Sts2.Core.CardSelection;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Nodes.GodotExtensions;
using MegaCrit.Sts2.Core.Nodes.CommonUi;
using MegaCrit.Sts2.Core.Nodes.Relics;
using MegaCrit.Sts2.Core.Nodes.Screens.Overlays;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Runs;

namespace KomeijiKoishi.Patches
{
    [HarmonyPatch(typeof(NRelicInventoryHolder), nameof(NRelicInventoryHolder._Ready))]
    public static class DataminerRelicPatch
    {
        private const string ConnectedMetaKey = "KoishiDataminerConnected";

        [HarmonyPostfix]
        public static void Postfix(NRelicInventoryHolder __instance)
        {
            if (__instance.HasMeta(ConnectedMetaKey))
            {
                return;
            }

            __instance.SetMeta(ConnectedMetaKey, true);
            __instance.Connect(
                NClickableControl.SignalName.MouseReleased,
                Callable.From<InputEvent>(inputEvent => OnMouseReleased(__instance, inputEvent)),
                0U);
        }

        private static void OnMouseReleased(NRelicInventoryHolder holder, InputEvent inputEvent)
        {
            if (inputEvent is not InputEventMouseButton { ButtonIndex: MouseButton.Right, Pressed: false })
            {
                return;
            }

            if (holder.Relic?.Model is not Dataminer_Koishi relic)
            {
                return;
            }

            if (!relic.CanTrigger)
            {
                return;
            }

            Player? owner = relic.Owner;
            if (owner == null || RunManager.Instance == null)
            {
                return;
            }

            int relicOrdinal = -1;
            int dataminerCount = 0;
            for (int i = 0; i < owner.Relics.Count; i++)
            {
                if (owner.Relics[i] is not Dataminer_Koishi)
                {
                    continue;
                }

                if (ReferenceEquals(owner.Relics[i], relic))
                {
                    relicOrdinal = dataminerCount;
                    break;
                }

                dataminerCount++;
            }

            if (relicOrdinal < 0)
            {
                return;
            }

            // The activation must go through the native action synchronizer.
            // Otherwise every multiplayer peer randomizes locally before the
            // resulting CardCmd.Transform calls are synchronized.
            RunManager.Instance.ActionQueueSynchronizer.RequestEnqueue(
                new DataminerRandomizeGameAction(owner, relicOrdinal));
        }

        internal static string CreateRandomizationPlan(Player owner, int relicOrdinal)
        {
            CombatState? combatState = owner.Creature.CombatState as CombatState;
            if (combatState == null)
            {
                return string.Empty;
            }

            List<RandomizedCardPlan> cards = new();
            foreach (CardModel _ in PileType.Hand.GetPile(owner).Cards)
            {
                DataminerEffect effect = DataminerEffect.Create(owner);
                int variant = owner.RunState.Rng.CombatCardGeneration.NextInt(7);
                int keywordMask = CreateKeywordMask(owner);
                cards.Add(new RandomizedCardPlan
                {
                    Effect = JsonSerializer.Serialize(effect),
                    Variant = variant,
                    KeywordMask = keywordMask
                });
            }

            List<int> powerChanges = new();
            foreach (PowerModel _ in owner.Creature.Powers)
            {
                powerChanges.Add(owner.RunState.Rng.CombatCardGeneration.NextInt(-5, 4));
            }

            return JsonSerializer.Serialize(new RandomizationPlan
            {
                Cards = cards,
                PowerChanges = powerChanges
            });
        }

        internal static async Task RandomizeHandForAction(
            Player owner,
            int relicOrdinal,
            string serializedPlan)
        {
            try
            {
                CombatState? combatState = owner.Creature.CombatState as CombatState;
                Dataminer_Koishi? relic = owner.Relics
                    .OfType<Dataminer_Koishi>()
                    .ElementAtOrDefault(relicOrdinal);
                if (combatState == null || relic == null || !relic.CanTrigger)
                {
                    return;
                }

                MegaCrit.Sts2.Core.Logging.Log.Info(
                    $"[KoishiDataminer] Activating copy {relicOrdinal + 1} for player {owner.NetId}; cooldown={relic.Cooldown}");

                List<CardModel> cards = PileType.Hand.GetPile(owner).Cards
                    .ToList();

                if (cards.Count == 0)
                {
                    return;
                }

                RandomizationPlan? plan = null;
                if (!string.IsNullOrEmpty(serializedPlan))
                {
                    plan = JsonSerializer.Deserialize<RandomizationPlan>(serializedPlan);
                }

                if (plan == null || plan.Cards.Count != cards.Count)
                {
                    MegaCrit.Sts2.Core.Logging.Log.Error(
                        $"[KoishiDataminer] Invalid synchronized plan: cards={cards.Count}, plan={plan?.Cards.Count ?? -1}.");
                    return;
                }

                relic.StartCooldown();

                for (int i = 0; i < cards.Count; i++)
                {
                    CardModel original = cards[i];
                    RandomizedCardPlan cardPlan = plan.Cards[i];
                    DataminerCard_Koishi replacement = CreateDataminerCard(owner, combatState, cardPlan.Variant);
                    DataminerEffect generatedEffect = JsonSerializer.Deserialize<DataminerEffect>(cardPlan.Effect);
                    replacement.SetEffect(generatedEffect);
                    AddKeywordsFromMask(replacement, cardPlan.KeywordMask);
                    await CardCmd.Transform(original, replacement, CardPreviewStyle.None);
                    // Transform can copy the source card's energy state onto the replacement.
                    // Reapply the generated effect after the card is in its final pile.
                    replacement.SetEffect(generatedEffect);
                    MegaCrit.Sts2.Core.Logging.Log.Info(
                        $"[KoishiDataminer] Generated transformed card cost={generatedEffect.Cost}");
                }

                await ApplyPowerChanges(owner, plan.PowerChanges);

                relic.Flash();
            }
            catch (Exception e)
            {
                MegaCrit.Sts2.Core.Logging.Log.Error($"[KoishiDataminer] Failed to randomize hand: {e}");
            }
        }

        private static DataminerCard_Koishi CreateDataminerCard(
            Player owner,
            CombatState combatState,
            int variant)
        {
            return variant switch
            {
                0 => combatState.CreateCard<DataminerCard_Koishi>(owner),
                1 => combatState.CreateCard<DataminerCard_Koishi_2>(owner),
                2 => combatState.CreateCard<DataminerCard_Koishi_3>(owner),
                3 => combatState.CreateCard<DataminerCard_Koishi_4>(owner),
                4 => combatState.CreateCard<DataminerCard_Koishi_5>(owner),
                5 => combatState.CreateCard<DataminerCard_Koishi_6>(owner),
                _ => combatState.CreateCard<DataminerCard_Koishi_7>(owner)
            };
        }

        private static async Task ApplyPowerChanges(Player owner, IReadOnlyList<int> changes)
        {
            List<PowerModel> powers = owner.Creature.Powers.ToList();

            for (int i = 0; i < powers.Count && i < changes.Count; i++)
            {
                PowerModel power = powers[i];
                int amountChange = changes[i];
                if (amountChange == 0)
                {
                    continue;
                }

                await PowerCmd.ModifyAmount(
                    new ThrowingPlayerChoiceContext(),
                    power,
                    amountChange,
                    owner.Creature,
                    null,
                    false);
            }
        }

        private static int CreateKeywordMask(Player owner)
        {
            var rng = owner.RunState.Rng.CombatCardGeneration;
            int mask = 0;
            int keywordCount = rng.NextInt(3);
            List<int> keywordBits = new() { 1, 2, 4 };
            for (int i = 0; i < keywordCount && keywordBits.Count > 0; i++)
            {
                int index = rng.NextInt(keywordBits.Count);
                mask |= keywordBits[index];
                keywordBits.RemoveAt(index);
            }
            return mask;
        }

        private static void AddKeywordsFromMask(DataminerCard_Koishi card, int mask)
        {
            if ((mask & 1) != 0) card.AddKeyword(CardKeyword.Exhaust);
            if ((mask & 2) != 0) card.AddKeyword(CardKeyword.Ethereal);
            if ((mask & 4) != 0) card.AddKeyword(CardKeyword.Sly);
        }

        private sealed class RandomizationPlan
        {
            public List<RandomizedCardPlan> Cards { get; set; } = new();
            public List<int> PowerChanges { get; set; } = new();
        }

        private sealed class RandomizedCardPlan
        {
            public string Effect { get; set; } = string.Empty;
            public int Variant { get; set; }
            public int KeywordMask { get; set; }
        }
    }
}
