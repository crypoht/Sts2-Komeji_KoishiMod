using Godot;
using MegaCrit.Sts2.addons.mega_text;
using MegaCrit.Sts2.Core.Localization;

namespace KomeijiKoishi.Config;

public partial class AncientProbabilityChart : Control
{
    private static readonly StringName ShadowOffsetX = "shadow_offset_x";
    private static readonly StringName ShadowOffsetY = "shadow_offset_y";

    private const float ChartHeight = 230f;
    private const float LeftPadding = 64f;
    private const float RightPadding = 24f;
    private const float TopPadding = 24f;
    private const float BottomPadding = 54f;
    private const float BarWidth = 74f;
    private const float BarGap = 92f;

    private readonly List<SegmentHitbox> _segments = new();
    private SegmentHitbox? _hoveredSegment;
    private Label? _tooltip;

    public AncientProbabilityChart()
    {
        Name = "AncientProbabilityChart";
        CustomMinimumSize = new Vector2(0, 330);
        SizeFlagsHorizontal = SizeFlags.ExpandFill;
        MouseFilter = MouseFilterEnum.Stop;
        MouseExited += OnMouseExited;
    }

    public void Refresh()
    {
        _hoveredSegment = null;
        HideTooltip();
        QueueRedraw();
    }

    public override void _Draw()
    {
        _segments.Clear();

        Vector2 size = Size;
        Rect2 plotRect = new(
            LeftPadding,
            TopPadding,
            Mathf.Max(1f, size.X - LeftPadding - RightPadding),
            ChartHeight);

        DrawAxes(plotRect);

        float totalBarWidth = 3 * BarWidth + 2 * BarGap;
        float startX = plotRect.Position.X + Mathf.Max(0f, (plotRect.Size.X - totalBarWidth) / 2f);

        for (int act = 1; act <= 3; act++)
        {
            float x = startX + (act - 1) * (BarWidth + BarGap);
            DrawActBar(act, new Rect2(x, plotRect.Position.Y, BarWidth, plotRect.Size.Y));
        }
    }

    public override void _GuiInput(InputEvent @event)
    {
        if (@event is InputEventMouseMotion motion)
        {
            UpdateHoveredSegment(motion.Position);
        }
    }

    private void OnMouseExited()
    {
        _hoveredSegment = null;
        HideTooltip();
        QueueRedraw();
    }

    private void DrawAxes(Rect2 plotRect)
    {
        Color axis = Color.FromHtml("#9FB0B8");
        Color grid = new(axis, 0.3f);
        Font? font = GetThemeFont(ThemeConstants.Label.Font, "Label");
        int fontSize = Math.Max(16, GetThemeFontSize(ThemeConstants.Label.FontSize, "Label") - 4);

        DrawLine(plotRect.Position, plotRect.Position + new Vector2(0, plotRect.Size.Y), axis, 2f);
        DrawLine(plotRect.Position + new Vector2(0, plotRect.Size.Y), plotRect.End, axis, 2f);

        for (int i = 0; i <= 4; i++)
        {
            float ratio = i / 4f;
            float y = plotRect.Position.Y + plotRect.Size.Y * (1f - ratio);
            DrawLine(new Vector2(plotRect.Position.X - 6f, y), new Vector2(plotRect.End.X, y), i == 0 ? axis : grid, 1f);
            if (font != null)
            {
                string label = $"{ratio * 100f:0}%";
                DrawString(font, new Vector2(8f, y + 6f), label, HorizontalAlignment.Left, -1, fontSize, Color.FromHtml("#C7D4D8"));
            }
        }
    }

    private void DrawActBar(int actNumber, Rect2 barRect)
    {
        IReadOnlyList<AncientProbabilityEntry> entries = AncientProbabilityData.GetActEntries(actNumber);
        Rect2 outlineRect = new(barRect.Position, barRect.Size);

        DrawRect(outlineRect, new Color(0.05f, 0.07f, 0.08f, 0.45f), false, 2f);

        float y = barRect.End.Y;
        foreach (AncientProbabilityEntry entry in entries)
        {
            float height = barRect.Size.Y * entry.Probability;
            y -= height;
            Rect2 segmentRect = new(barRect.Position.X, y, barRect.Size.X, height);
            Color color = entry.Info.Color;
            DrawRect(segmentRect, color);
            DrawRect(segmentRect, new Color(0f, 0f, 0f, 0.18f), false, 1f);
            _segments.Add(new SegmentHitbox(segmentRect, actNumber, entry));
        }

        if (_hoveredSegment is { } hovered)
        {
            DrawRect(hovered.Rect, Colors.White, false, 3f);
        }

        DrawActLabel(actNumber, barRect);
    }

    private void DrawActLabel(int actNumber, Rect2 barRect)
    {
        Font? font = GetThemeFont(ThemeConstants.Label.Font, "Label");
        if (font == null)
        {
            return;
        }

        int fontSize = Math.Max(18, GetThemeFontSize(ThemeConstants.Label.FontSize, "Label"));
        string label = actNumber.ToString();
        Vector2 labelSize = font.GetStringSize(label, HorizontalAlignment.Left, -1, fontSize);
        DrawString(
            font,
            new Vector2(barRect.Position.X + (barRect.Size.X - labelSize.X) / 2f, barRect.End.Y + 34f),
            label,
            HorizontalAlignment.Left,
            -1,
            fontSize,
            Color.FromHtml("#DDE7EA"));
    }

    private void UpdateHoveredSegment(Vector2 localPosition)
    {
        SegmentHitbox? next = _segments.FirstOrDefault(segment => segment.Rect.HasPoint(localPosition));
        if (Equals(next, _hoveredSegment))
        {
            return;
        }

        _hoveredSegment = next;
        HideTooltip();
        QueueRedraw();

        if (next == null)
        {
            return;
        }

        string title = FormatTooltipTitle(next.Entry.Info, next.ActNumber);
        string description = FormatTooltipDescription(next.Entry);
        ShowTooltip(title + "\n" + description);
    }

    public static string LocalizedAncientName(AncientProbabilityInfo info)
    {
        return LocString
            .GetIfExists("settings_ui", $"KOMEIJIKOISHI-ANCIENT_PROBABILITY.{info.Key}.name")
            ?.GetFormattedText() ?? info.DisplayName;
    }

    public static string LocalizedNone()
    {
        return LocString
            .GetIfExists("settings_ui", "KOMEIJIKOISHI-ANCIENT_PROBABILITY.none")
            ?.GetFormattedText() ?? "None";
    }

    private static string FormatTooltipTitle(AncientProbabilityInfo info, int actNumber)
    {
        string template = LocString
            .GetIfExists("settings_ui", "KOMEIJIKOISHI-ANCIENT_PROBABILITY.tooltip.title")
            ?.GetFormattedText() ?? "{AncientName} - Act {ActNumber}";
        return template
            .Replace("{AncientName}", LocalizedAncientName(info))
            .Replace("{ActNumber}", actNumber.ToString());
    }

    private static string FormatTooltipDescription(AncientProbabilityEntry entry)
    {
        string template = LocString
            .GetIfExists("settings_ui", "KOMEIJIKOISHI-ANCIENT_PROBABILITY.tooltip.description")
            ?.GetFormattedText() ?? "Appearance chance: {Probability}%\nWeight: {Weight}";
        return template
            .Replace("{Probability}", $"{entry.Probability * 100f:0.#}")
            .Replace("{Weight}", entry.Weight.ToString());
    }

    private void ShowTooltip(string text)
    {
        _tooltip ??= CreateTooltip();
        _tooltip.Text = text;
        _tooltip.Visible = true;
        _tooltip.GlobalPosition = GetGlobalMousePosition() + new Vector2(18f, -28f);
    }

    private Label CreateTooltip()
    {
        Label label = new()
        {
            Name = "AncientProbabilityTooltip",
            MouseFilter = MouseFilterEnum.Ignore,
            ZIndex = 100,
            AutowrapMode = TextServer.AutowrapMode.Off
        };
        label.AddThemeColorOverride(ThemeConstants.Label.FontColor, Color.FromHtml("#F2F8FA"));
        label.AddThemeColorOverride(ThemeConstants.Label.FontShadowColor, Color.FromHtml("#000000"));
        label.AddThemeConstantOverride(ShadowOffsetX, 2);
        label.AddThemeConstantOverride(ShadowOffsetY, 2);
        AddChild(label);
        return label;
    }

    private void HideTooltip()
    {
        if (_tooltip != null)
        {
            _tooltip.Visible = false;
        }
    }

    private sealed record SegmentHitbox(Rect2 Rect, int ActNumber, AncientProbabilityEntry Entry);
}
