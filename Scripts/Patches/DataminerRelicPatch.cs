using System;
using System.Collections.Generic;
using System.Linq;
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

        internal static async Task RandomizeHandForAction(Player owner, int relicOrdinal)
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

                relic.StartCooldown();

                foreach (CardModel original in cards)
                {
                    DataminerCard_Koishi replacement = CreateRandomDataminerCard(owner, combatState);
                    DataminerEffect generatedEffect = DataminerEffect.Create(owner);
                    replacement.SetEffect(generatedEffect);
                    AddRandomKeywords(replacement, owner);
                    await CardCmd.Transform(original, replacement, CardPreviewStyle.None);
                    // Transform can copy the source card's energy state onto the replacement.
                    // Reapply the generated effect after the card is in its final pile.
                    replacement.SetEffect(generatedEffect);
                    MegaCrit.Sts2.Core.Logging.Log.Info(
                        $"[KoishiDataminer] Generated transformed card cost={generatedEffect.Cost}");
                }

                await RandomizePowerAmounts(owner);

                relic.Flash();
            }
            catch (Exception e)
            {
                MegaCrit.Sts2.Core.Logging.Log.Error($"[KoishiDataminer] Failed to randomize hand: {e}");
            }
        }

        private static DataminerCard_Koishi CreateRandomDataminerCard(Player owner, CombatState combatState)
        {
            return owner.RunState.Rng.CombatCardGeneration.NextInt(7) switch
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

        private static async Task RandomizePowerAmounts(Player owner)
        {
            var rng = owner.RunState.Rng.CombatCardGeneration;
            List<PowerModel> powers = owner.Creature.Powers.ToList();

            foreach (PowerModel power in powers)
            {
                int amountChange = rng.NextInt(-5, 4);
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

        private static void AddRandomKeywords(DataminerCard_Koishi card, Player owner)
        {
            var rng = owner.RunState.Rng.CombatCardGeneration;
            int keywordCount = rng.NextInt(3);
            List<CardKeyword> keywords = new List<CardKeyword>
            {
                CardKeyword.Exhaust,
                CardKeyword.Ethereal,
                CardKeyword.Sly
            };

            for (int i = 0; i < keywordCount && keywords.Count > 0; i++)
            {
                int index = rng.NextInt(keywords.Count);
                card.AddKeyword(keywords[index]);
                keywords.RemoveAt(index);
            }
        }
    }
}
