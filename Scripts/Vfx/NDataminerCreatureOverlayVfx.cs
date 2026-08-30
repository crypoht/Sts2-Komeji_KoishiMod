using System.Collections.Generic;
using Godot;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.Nodes.Combat;
using MegaCrit.Sts2.Core.Nodes.Rooms;

namespace KomeijiKoishi.Vfx;

public sealed partial class NDataminerCreatureOverlayVfx : Node2D
{
    private const float BottomExtension = 40f;
    private static readonly Dictionary<ulong, NDataminerCreatureOverlayVfx> ActiveOverlays = new();

    private NCreature? targetNode;
    private Texture2D? texture;
    private Sprite2D? sprite;
    private ulong targetId;

    public static NDataminerCreatureOverlayVfx? Create(Creature target, string portraitPath)
    {
        NCreature? node = NCombatRoom.Instance?.GetCreatureNode(target);
        if (node == null || !ResourceLoader.Exists(portraitPath))
        {
            return null;
        }

        NDataminerCreatureOverlayVfx overlay = new()
        {
            targetNode = node,
            texture = ResourceLoader.Load<Texture2D>(portraitPath),
            targetId = node.GetInstanceId()
        };
        node.AddChildSafely(overlay);
        return overlay;
    }

    public override void _Ready()
    {
        if (targetNode == null || texture == null)
        {
            this.QueueFreeSafely();
            return;
        }

        if (ActiveOverlays.TryGetValue(targetId, out NDataminerCreatureOverlayVfx? oldOverlay)
            && GodotObject.IsInstanceValid(oldOverlay))
        {
            oldOverlay.QueueFreeSafely();
        }

        ActiveOverlays[targetId] = this;
        ZIndex = 1;
        sprite = new Sprite2D
        {
            Texture = texture,
            Centered = true,
            ZIndex = 0,
            Modulate = Colors.White
        };
        AddChild(sprite);
        UpdateTransform();
    }

    public override void _Process(double delta)
    {
        if (targetNode == null || !GodotObject.IsInstanceValid(targetNode) || GetParent() != targetNode || NCombatRoom.Instance == null)
        {
            this.QueueFreeSafely();
            return;
        }

        UpdateTransform();
    }

    public override void _ExitTree()
    {
        if (ActiveOverlays.TryGetValue(targetId, out NDataminerCreatureOverlayVfx? current) && current == this)
        {
            ActiveOverlays.Remove(targetId);
        }

        base._ExitTree();
    }

    private void UpdateTransform()
    {
        if (targetNode == null || texture == null || sprite == null || texture.GetWidth() <= 0 || texture.GetHeight() <= 0)
        {
            return;
        }

        Vector2 hitboxPosition = targetNode.Hitbox.GlobalPosition;
        Vector2 hitboxSize = targetNode.Hitbox.Size;
        float width = hitboxSize.X;
        float height = hitboxSize.Y + BottomExtension;
        GlobalPosition = hitboxPosition + new Vector2(width * 0.5f, height * 0.5f);
        sprite.Scale = new Vector2(width / texture.GetWidth(), height / texture.GetHeight());
    }
}
