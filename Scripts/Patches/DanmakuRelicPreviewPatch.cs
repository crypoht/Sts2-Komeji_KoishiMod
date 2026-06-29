using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Godot;
using HarmonyLib;
using MegaCrit.Sts2.addons.mega_text;
using KomeijiKoishi.Cards;
using KomeijiKoishi.Cards.Danmaku;
using KomeijiKoishi.Relics;
using MegaCrit.Sts2.Core.CardSelection;
using MegaCrit.Sts2.Core.ControllerInput;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Nodes;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.Nodes.GodotExtensions;
using MegaCrit.Sts2.Core.Nodes.Relics;
using MegaCrit.Sts2.Core.Nodes.Screens.CardSelection;
using MegaCrit.Sts2.Core.Nodes.Screens.Overlays;

namespace KomeijiKoishi.Patches
{
    [HarmonyPatch(typeof(NRelicInventoryHolder), nameof(NRelicInventoryHolder._Ready))]
    public static class DanmakuRelicPreviewPatch
    {
        private const string ConnectedMetaKey = "KoishiDanmakuPreviewConnected";
        private static NSimpleCardSelectScreen? _currentPreviewScreen;

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
            try
            {
                if (inputEvent is not InputEventMouseButton { ButtonIndex: MouseButton.Right, Pressed: false })
                {
                    return;
                }

                if (holder.Relic?.Model is not KoishiStarterRelic and not KoishiAnicentRelic)
                {
                    return;
                }

                if (IsPreviewOpen())
                {
                    ClosePreview();
                    return;
                }

                Player? owner = holder.Relic.Model.Owner;
                if (owner == null || NOverlayStack.Instance == null)
                {
                    return;
                }

                List<CardModel> cards = CreatePreviewCards(owner);
                if (cards.Count == 0)
                {
                    return;
                }

                CardSelectorPrefs prefs = new CardSelectorPrefs(new LocString("gameplay_ui", "KOMEIJIKOISHI-DANMAKU_PREVIEW.prompt"), 0, 0)
                {
                    RequireManualConfirmation = true,
                    Cancelable = true
                };

                _currentPreviewScreen = NSimpleCardSelectScreen.Create(cards, prefs);
                NOverlayStack.Instance.Push(_currentPreviewScreen);
                _ = ApplyPreviewPrompt(_currentPreviewScreen);
            }
            catch (Exception e)
            {
                MegaCrit.Sts2.Core.Logging.Log.Error($"[KoishiDanmakuPreview] Failed to open Danmaku preview: {e}");
            }
        }

        private static async Task ApplyPreviewPrompt(NSimpleCardSelectScreen screen)
        {
            await screen.ToSignal(screen.GetTree(), SceneTree.SignalName.ProcessFrame);

            string prompt = new LocString("gameplay_ui", "KOMEIJIKOISHI-DANMAKU_PREVIEW.prompt").GetFormattedText();

            MegaRichTextLabel? bottomLabel = screen.GetNodeOrNull<MegaRichTextLabel>("%BottomLabel");
            if (bottomLabel != null)
            {
                bottomLabel.Text = "[center]" + prompt + "[/center]";
            }

            if (screen.GetNodeOrNull<Label>("KoishiDanmakuPreviewTopPrompt") != null)
            {
                return;
            }

            Label topLabel = new Label
            {
                Name = "KoishiDanmakuPreviewTopPrompt",
                Text = prompt,
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center,
                MouseFilter = Control.MouseFilterEnum.Ignore,
                ZIndex = 1000
            };
            topLabel.SetAnchorsPreset(Control.LayoutPreset.TopWide);
            topLabel.OffsetTop = 26f;
            topLabel.OffsetBottom = 86f;
            topLabel.AddThemeColorOverride(ThemeConstants.Label.FontColor, Colors.White);
            topLabel.AddThemeColorOverride(ThemeConstants.Label.FontShadowColor, Colors.Black);
            topLabel.AddThemeConstantOverride("shadow_offset_x", 2);
            topLabel.AddThemeConstantOverride("shadow_offset_y", 2);
            topLabel.AddThemeFontSizeOverride(ThemeConstants.Label.FontSize, 28);

            screen.AddChildSafely(topLabel);
        }

        public static bool IsPreviewOpen()
        {
            return _currentPreviewScreen != null &&
                   NOverlayStack.Instance != null &&
                   ReferenceEquals(NOverlayStack.Instance.Peek(), _currentPreviewScreen);
        }

        public static bool ClosePreview()
        {
            if (_currentPreviewScreen == null || NOverlayStack.Instance == null)
            {
                _currentPreviewScreen = null;
                return false;
            }

            try
            {
                NOverlayStack.Instance.Remove(_currentPreviewScreen);
                return true;
            }
            catch (Exception e)
            {
                MegaCrit.Sts2.Core.Logging.Log.Error($"[KoishiDanmakuPreview] Failed to close Danmaku preview: {e}");
                return false;
            }
            finally
            {
                _currentPreviewScreen = null;
            }
        }

        private static List<CardModel> CreatePreviewCards(Player owner)
        {
            List<CardModel> cards = new List<CardModel>
            {
                CreatePreviewCard<HeartDanmaku_Koishi>(owner),
                CreatePreviewCard<StarDanmaku_Koishi>(owner),
                CreatePreviewCard<YinYangOrbDanmaku_Koishi>(owner),
                CreatePreviewCard<RiceDanmaku_Koishi>(owner),
                CreatePreviewCard<SmallOrbDanmaku_Koishi>(owner),
                CreatePreviewCard<LargeOrbDanmaku_Koishi>(owner),
                CreatePreviewCard<ArrowDanmaku_Koishi>(owner),
                CreatePreviewCard<SquareDanmaku_Koishi>(owner),
                CreatePreviewCard<RoseDanmaku_Koishi>(owner)
            };

            return cards;
        }

        private static CardModel CreatePreviewCard<T>(Player owner) where T : CardModel
        {
            CardModel card = owner.RunState.CreateCard<T>(owner);
            owner.RunState.RemoveCard(card);
            return card;
        }
    }

    [HarmonyPatch(typeof(NGame), nameof(NGame._Input))]
    public static class DanmakuRelicPreviewEscapePatch
    {
        [HarmonyPostfix]
        public static void Postfix(InputEvent inputEvent)
        {
            if (!DanmakuRelicPreviewPatch.IsPreviewOpen())
            {
                return;
            }

            bool shouldClose = inputEvent.IsActionPressed(MegaInput.cancel, false, false) ||
                               inputEvent is InputEventKey { Pressed: true, Echo: false, Keycode: Key.Escape };

            if (shouldClose)
            {
                DanmakuRelicPreviewPatch.ClosePreview();
            }
        }
    }
}
