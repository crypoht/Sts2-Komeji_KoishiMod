using Godot;
using System.Collections.Generic;
using KomeijiKoishi.Cards.Fumo;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.Logging;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Nodes.Combat;
using MegaCrit.Sts2.Core.Nodes.Rooms;

namespace KomeijiKoishi.Vfx;

public sealed partial class NGiftYouFumoVfx : Node2D
{
    public const float FlightDuration = 0.72f;
    public const float PeakHeight = 150f;
    public const float HeadOffsetY = -110f;
    public const float VisualWidth = 96f;
    public const float SpinSpeed = 9f;
    public const float FootAnchorOffsetY = 4f;
    public const float InnerRowSlotOffsetX = 38f;
    public const float OuterRowSlotOffsetX = 24f;
    public const float OuterRowOffsetY = 30f;
    public const float IdleSquashSpeed = 3.4666f;
    public const float IdleSquashWidth = 1.1f;
    public const float IdleSquashHeight = 0.9167f;

    private Vector2 startPosition;
    private Vector2 endPosition;
    private Vector2 velocity;
    private float gravity;
    private float elapsed;
    private string texturePath = string.Empty;
    private NCreature? targetNode;
    private int targetSlot;
    private Sprite2D? sprite;
    private Texture2D? texture;

    private static readonly Dictionary<ulong, TargetFumoSlots> TargetSlots = new();

    public static NGiftYouFumoVfx? Create(Creature source, Creature target, CardModel fumoCard)
    {
        string? path = GetTexturePath(fumoCard);
        if (path == null)
        {
            Log.Warn($"[KoishiGiftYouFumoVfx] Missing fumo texture mapping for {fumoCard.GetType().Name}.");
            return null;
        }

        if (!ResourceLoader.Exists(path))
        {
            Log.Warn($"[KoishiGiftYouFumoVfx] Missing texture: {path}");
            return null;
        }

        NCombatRoom? room = NCombatRoom.Instance;
        NCreature? sourceNode = room?.GetCreatureNode(source);
        NCreature? targetNode = room?.GetCreatureNode(target);
        if (sourceNode == null || targetNode == null)
        {
            Log.Warn($"[KoishiGiftYouFumoVfx] Missing combat node. sourceNode={sourceNode != null}, targetNode={targetNode != null}");
            return null;
        }

        int targetSlot = GetNextTargetSlotIndex(targetNode);
        Vector2 start = GetHeadPosition(sourceNode);
        Vector2 end = GetTargetAnchorPosition(targetNode, targetSlot);
        return new NGiftYouFumoVfx
        {
            startPosition = start,
            endPosition = end,
            texturePath = path,
            targetNode = targetNode,
            targetSlot = targetSlot
        };
    }

    public override void _Ready()
    {
        GlobalPosition = startPosition;
        SolveGravityArc();

        texture = ResourceLoader.Load<Texture2D>(texturePath);
        sprite = new Sprite2D
        {
            Texture = texture,
            Centered = true,
            ZIndex = 150,
            Modulate = Colors.White
        };

        if (texture.GetWidth() > 0)
        {
            sprite.Scale = Vector2.One * (VisualWidth / texture.GetWidth());
        }

        AddChild(sprite);
    }

    public override void _Process(double delta)
    {
        elapsed += (float)delta;
        float t = Mathf.Clamp(elapsed / FlightDuration, 0f, 1f);
        float time = FlightDuration * t;
        GlobalPosition = startPosition + velocity * time + Vector2.Down * (0.5f * gravity * time * time);

        if (sprite != null)
        {
            sprite.Rotation += SpinSpeed * (float)delta;
        }

        if (t >= 1f)
        {
            AttachPersistentFumo();
            this.QueueFreeSafely();
        }
    }

    private void SolveGravityArc()
    {
        float totalTime = FlightDuration;
        float peakTime = totalTime * 0.5f;
        float apexY = Mathf.Min(startPosition.Y, endPosition.Y) - PeakHeight;
        float apexDeltaY = apexY - startPosition.Y;
        float endDeltaY = endPosition.Y - startPosition.Y;
        float denominator = totalTime * totalTime - totalTime * peakTime;

        gravity = denominator != 0f
            ? 2f * (endDeltaY - apexDeltaY * (totalTime / peakTime)) / denominator
            : 0f;

        float velocityY = peakTime != 0f
            ? (apexDeltaY - 0.5f * gravity * peakTime * peakTime) / peakTime
            : 0f;

        velocity = new Vector2((endPosition.X - startPosition.X) / totalTime, velocityY);
    }

    private static Vector2 GetHeadPosition(NCreature creatureNode)
    {
        return creatureNode.Hitbox.GlobalPosition + new Vector2(creatureNode.Hitbox.Size.X * 0.5f, HeadOffsetY);
    }

    private void AttachPersistentFumo()
    {
        if (texture == null || targetNode == null || !GodotObject.IsInstanceValid(targetNode))
        {
            return;
        }

        ulong targetId = targetNode.GetInstanceId();
        if (!TargetSlots.TryGetValue(targetId, out TargetFumoSlots? slots) || !slots.IsValid)
        {
            slots = new TargetFumoSlots(targetNode);
            TargetSlots[targetId] = slots;
        }

        slots.Add(texture, targetSlot);
    }

    private static int GetNextTargetSlotIndex(NCreature target)
    {
        ulong targetId = target.GetInstanceId();
        if (!TargetSlots.TryGetValue(targetId, out TargetFumoSlots? slots) || !slots.IsValid)
        {
            return 0;
        }

        return slots.NextSlotIndex;
    }

    private sealed class TargetFumoSlots
    {
        private readonly NCreature target;
        private readonly PersistentFumoMarker?[] markers = new PersistentFumoMarker?[5];
        private int nextSlot;

        public TargetFumoSlots(NCreature target)
        {
            this.target = target;
        }

        public bool IsValid => GodotObject.IsInstanceValid(target);

        public int NextSlotIndex => nextSlot;

        public void Add(Texture2D texture, int slot)
        {
            nextSlot = (slot + 1) % markers.Length;

            markers[slot]?.QueueFreeSafely();

            PersistentFumoMarker marker = new(target, texture, slot);
            markers[slot] = marker;
            NCombatRoom.Instance?.CombatVfxContainer.AddChildSafely(marker);
        }
    }

    private sealed partial class PersistentFumoMarker : Node2D
    {
        private readonly NCreature target;
        private readonly Texture2D texture;
        private readonly int slot;
        private readonly float phase;
        private Sprite2D? sprite;
        private float baseSpriteScale = 1f;
        private float baseVisualHeight;
        private float elapsed;

        public PersistentFumoMarker(NCreature target, Texture2D texture, int slot)
        {
            this.target = target;
            this.texture = texture;
            this.slot = slot;
            phase = (slot + 1) * 1.37f;
        }

        public override void _Ready()
        {
            sprite = new Sprite2D
            {
                Texture = texture,
                Centered = true,
                ZIndex = 120 + slot,
                Modulate = Colors.White
            };

            if (texture.GetWidth() > 0)
            {
                baseSpriteScale = VisualWidth / texture.GetWidth();
                baseVisualHeight = texture.GetHeight() * baseSpriteScale;
                sprite.Scale = Vector2.One * baseSpriteScale;
                sprite.Position = new Vector2(0f, -baseVisualHeight * 0.5f);
            }

            AddChild(sprite);
            UpdatePosition(0f);
        }

        public override void _Process(double delta)
        {
            elapsed += (float)delta;
            if (!GodotObject.IsInstanceValid(target))
            {
                this.QueueFreeSafely();
                return;
            }

            UpdatePosition((float)delta);
        }

        private void UpdatePosition(float delta)
        {
            Vector2 anchor = GetAnchorPosition();
            GlobalPosition = anchor;

            if (sprite != null)
            {
                float pulse = (Mathf.Sin(elapsed * IdleSquashSpeed + phase) + 1f) * 0.5f;
                float scaleX = Mathf.Lerp(1f, IdleSquashWidth, pulse);
                float scaleY = Mathf.Lerp(1f, IdleSquashHeight, pulse);
                sprite.Scale = new Vector2(baseSpriteScale * scaleX, baseSpriteScale * scaleY);
                sprite.Position = new Vector2(0f, -baseVisualHeight * scaleY * 0.5f);
            }
        }

        private Vector2 GetAnchorPosition()
        {
            return GetTargetAnchorPosition(target, slot);
        }
    }

    private static Vector2 GetTargetAnchorPosition(NCreature target, int slot)
    {
        Vector2 hitboxPosition = target.Hitbox.GlobalPosition;
        Vector2 hitboxSize = target.Hitbox.Size;
        Vector2 center = hitboxPosition + new Vector2(hitboxSize.X * 0.5f, 0f);
        float footY = hitboxSize.Y + FootAnchorOffsetY;

        return slot switch
        {
            0 => center + new Vector2(-InnerRowSlotOffsetX, footY),
            1 => center + new Vector2(0f, footY),
            2 => center + new Vector2(InnerRowSlotOffsetX, footY),
            3 => center + new Vector2(-OuterRowSlotOffsetX, footY + OuterRowOffsetY),
            _ => center + new Vector2(OuterRowSlotOffsetX, footY + OuterRowOffsetY)
        };
    }

    private static string? GetTexturePath(CardModel card)
    {
        string? fileName = card switch
        {
            CirnoFumo_Koishi => "ciron.png",
            KogasaFumo_Koishi => "Kogasafumo.png",
            MarisaFumo_Koishi => "Marisafumo.png",
            OkinaFumo_Koishi => "Okinafumo.png",
            ReimuFumo_Koishi => "Reimufumo.png",
            ReisenFumo_Koishi => "reisenfumo.png",
            TewiFumo_Koishi => "Tewifumo.png",
            YukariFumo_Koishi => "Yukarifumo.png",
            YuukaFumo_Koishi => "Yuukafumo.png",
            FlandreFumo_Koishi => "Flanderfumo.png",
            LWKoishiFumo_Koishi => "LWKoishifumo.png",
            ShionFumo_Koishi => "shionfumo.png",
            YoumuFumo_Koishi => "Youmufumo.png",
            NueFumo_Koishi => "Nuefumo.png",
            YuyukoFumo_Koishi => "Yuyukofumo.png",
            _ => null
        };

        return fileName == null
            ? null
            : $"res://mods/Komeiji_Koishi/images/qingxu/{fileName}";
    }
}
