using Godot;
using System.Collections.Generic;
using System.Linq;
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
    public const float InnerRowSlotOffsetX = 57f;
    public const float OuterRowSlotOffsetX = 36f;
    public const float OuterRowOffsetY = 30f;
    public const float IdleSquashSpeed = 3.4666f;
    public const float IdleSquashWidth = 1.1f;
    public const float IdleSquashHeight = 0.9167f;
    public const float SecondaryFlightDelay = 0.5f;
    public const float SecondaryGap = 10f;

    private Vector2 startPosition;
    private Vector2 endPosition;
    private Vector2 velocity;
    private float gravity;
    private float elapsed;
    private string texturePath = string.Empty;
    private string? secondaryTexturePath;
    private NCreature? targetNode;
    private int targetSlot;
    private Sprite2D? sprite;
    private Sprite2D? secondarySprite;
    private Texture2D? texture;
    private Texture2D? secondaryTexture;
    private float primaryVisualWidth = VisualWidth;
    private float secondaryVisualWidth = VisualWidth;
    private bool primaryAttached;
    private PersistentFumoMarker? attachedMarker;

    private static readonly Dictionary<ulong, TargetFumoSlots> TargetSlots = new();
    private const string TextureDirectory = "res://mods/Komeiji_Koishi/images/qingxu/";
    private static readonly Dictionary<string, string> TextureFiles = new()
    {
        ["reimu"] = "Reimufumo.png",
        ["cirno"] = "ciron.png",
        ["kogasa"] = "Kogasafumo.png",
        ["marisa"] = "Marisafumo.png",
        ["okina"] = "Okinafumo.png",
        ["reisen"] = "reisenfumo.png",
        ["tewi"] = "Tewifumo.png",
        ["yukari"] = "Yukarifumo.png",
        ["yuuka"] = "Yuukafumo.png",
        ["flandre"] = "Flanderfumo.png",
        ["lwkoishi"] = "LWKoishifumo.png",
        ["shion"] = "shionfumo.png",
        ["youmu"] = "Youmufumo.png",
        ["nue"] = "Nuefumo.png",
        ["yuyuko"] = "Yuyukofumo.png",
        ["minamitu"] = "Minamitufumo.png",
        ["koakuma"] = "Koakumafumo.png",
        ["pinkkoishi"] = "PinkKoishifumo.png",
        ["marisamoon"] = "MarisaMoonfumo_fumo.png",
        ["clownpiece"] = "Clownpiecefumo.png",
        ["daiyousei"] = "Daiyouseifumo.png",
        ["junko"] = "Junkofumo.png",
        ["keiki"] = "Keikifumo.png",
        ["lizunamaru"] = "Lizunamarufumo.png",
        ["maribel"] = "Maribelfumo.png",
        ["marisapast"] = "MarisaPastfumo.png",
        ["reimumoon"] = "ReimuMoonfumo.png",
        ["reimupast"] = "ReimuPastfumo.png",
        ["renko"] = "Renkofumo.png",
        ["sanae"] = "Sanaefumo.png",
        ["satori"] = "Satorifumo.png",
        ["suwako"] = "Suwakofumo.png"
    };

    public static IReadOnlyList<string> TextureNames => TextureFiles.Keys.OrderBy(name => name).ToList();

    public static NGiftYouFumoVfx? Create(Creature source, Creature target, CardModel fumoCard)
    {
        string? path = GetTexturePath(fumoCard);
        if (path == null)
        {
            Log.Warn($"[KoishiGiftYouFumoVfx] Missing fumo texture mapping for {fumoCard.GetType().Name}.");
            return null;
        }

        return CreateFromPaths(source, target, path, GetSecondaryTexturePath(fumoCard));
    }

    public static NGiftYouFumoVfx? Create(Creature source, Creature target, string textureName)
    {
        if (!TryGetTexturePaths(textureName, out string? path, out string? secondaryPath) || path == null)
        {
            Log.Warn($"[KoishiGiftYouFumoVfx] Missing fumo texture mapping for '{textureName}'.");
            return null;
        }

        return CreateFromPaths(source, target, path, secondaryPath);
    }

    public static bool TryGetTexturePaths(string textureName, out string? primaryPath, out string? secondaryPath)
    {
        string normalizedName = NormalizeTextureName(textureName);
        if (!TextureFiles.TryGetValue(normalizedName, out string? fileName))
        {
            primaryPath = null;
            secondaryPath = null;
            return false;
        }

        primaryPath = TextureDirectory + fileName;
        secondaryPath = normalizedName == "marisamoon"
            ? TextureDirectory + "MarisaMoonfumo_gun.png"
            : null;
        return true;
    }

    private static NGiftYouFumoVfx? CreateFromPaths(Creature source, Creature target, string path, string? secondaryPath)
    {
        if (!ResourceLoader.Exists(path) || (secondaryPath != null && !ResourceLoader.Exists(secondaryPath)))
        {
            Log.Warn($"[KoishiGiftYouFumoVfx] Missing texture: primary={path}, secondary={secondaryPath ?? "none"}");
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
            secondaryTexturePath = secondaryPath,
            targetNode = targetNode,
            targetSlot = targetSlot
        };
    }

    public override void _Ready()
    {
        GlobalPosition = startPosition;
        SolveGravityArc();

        texture = ResourceLoader.Load<Texture2D>(texturePath);
        secondaryTexture = secondaryTexturePath == null ? null : ResourceLoader.Load<Texture2D>(secondaryTexturePath);
        sprite = new Sprite2D
        {
            Texture = texture,
            Centered = true,
            ZIndex = 20,
            Modulate = Colors.White
        };

        if (texture.GetWidth() > 0)
        {
            sprite.Scale = Vector2.One * (VisualWidth / texture.GetWidth());
            primaryVisualWidth = VisualWidth;
        }

        AddChild(sprite);

        if (secondaryTexture != null)
        {
            secondarySprite = new Sprite2D
            {
                Texture = secondaryTexture,
                Centered = true,
                ZIndex = 19,
                Modulate = Colors.White,
                Visible = false
            };

            if (secondaryTexture.GetWidth() > 0)
            {
                secondarySprite.Scale = Vector2.One * (VisualWidth / secondaryTexture.GetWidth());
                secondaryVisualWidth = VisualWidth;
            }

            AddChild(secondarySprite);
        }
    }

    public override void _Process(double delta)
    {
        elapsed += (float)delta;
        float t = Mathf.Clamp(elapsed / FlightDuration, 0f, 1f);
        float time = FlightDuration * t;
        GlobalPosition = startPosition + velocity * time + Vector2.Down * (0.5f * gravity * time * time);

        if (sprite != null && t < 1f)
        {
            sprite.Rotation += SpinSpeed * (float)delta;
        }

        if (secondarySprite != null)
        {
            UpdateSecondaryProjectile((float)delta);
        }

        if (t >= 1f && !primaryAttached)
        {
            attachedMarker = AttachPersistentFumo(secondarySprite == null);
            primaryAttached = true;
            if (sprite != null)
            {
                sprite.Visible = false;
            }
        }

        if (elapsed >= FlightDuration + (secondarySprite == null ? 0f : SecondaryFlightDelay))
        {
            attachedMarker?.ShowSecondary();
            this.QueueFreeSafely();
        }
    }

    private void UpdateSecondaryProjectile(float delta)
    {
        if (secondarySprite == null)
        {
            return;
        }

        float delayedElapsed = elapsed - SecondaryFlightDelay;
        if (delayedElapsed < 0f)
        {
            secondarySprite.Visible = false;
            return;
        }

        secondarySprite.Visible = true;
        float t = Mathf.Clamp(delayedElapsed / FlightDuration, 0f, 1f);
        float time = FlightDuration * t;
        Vector2 offset = GetSecondaryOffset();
        Vector2 secondaryStart = startPosition + offset;
        Vector2 secondaryEnd = endPosition + offset;
        Vector2 secondaryVelocity = new Vector2((secondaryEnd.X - secondaryStart.X) / FlightDuration, velocity.Y);
        Vector2 secondaryGlobalPosition = secondaryStart + secondaryVelocity * time + Vector2.Down * (0.5f * gravity * time * time);
        secondarySprite.Position = secondaryGlobalPosition - GlobalPosition;
        if (t < 1f)
        {
            secondarySprite.Rotation += SpinSpeed * delta;
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

    private PersistentFumoMarker? AttachPersistentFumo(bool showSecondary)
    {
        if (texture == null || targetNode == null || !GodotObject.IsInstanceValid(targetNode))
        {
            return null;
        }

        ulong targetId = targetNode.GetInstanceId();
        if (!TargetSlots.TryGetValue(targetId, out TargetFumoSlots? slots) || !slots.IsValid)
        {
            slots = new TargetFumoSlots(targetNode);
            TargetSlots[targetId] = slots;
        }

        return slots.Add(texture, secondaryTexture, targetSlot, showSecondary);
    }

    private Vector2 GetSecondaryOffset()
    {
        return new Vector2(primaryVisualWidth * 0.5f + SecondaryGap + secondaryVisualWidth * 0.5f, 0f);
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
        private readonly ulong targetId;
        private int nextSlot;

        public TargetFumoSlots(NCreature target)
        {
            this.target = target;
            targetId = target.GetInstanceId();
        }

        public bool IsValid => GodotObject.IsInstanceValid(target);

        public int NextSlotIndex => nextSlot;

        public PersistentFumoMarker Add(Texture2D texture, Texture2D? secondaryTexture, int slot, bool showSecondary)
        {
            nextSlot = (slot + 1) % markers.Length;

            markers[slot]?.QueueFreeSafely();

            PersistentFumoMarker marker = new(this, target, texture, secondaryTexture, slot, showSecondary);
            markers[slot] = marker;
            target.AddChildSafely(marker);
            return marker;
        }

        public void ClearSlot(int slot, PersistentFumoMarker marker)
        {
            if (slot < 0 || slot >= markers.Length || markers[slot] != marker)
            {
                return;
            }

            markers[slot] = null;
            if (IsEmpty)
            {
                TargetSlots.Remove(targetId);
            }
        }

        private bool IsEmpty
        {
            get
            {
                foreach (PersistentFumoMarker? marker in markers)
                {
                    if (marker != null && GodotObject.IsInstanceValid(marker))
                    {
                        return false;
                    }
                }

                return true;
            }
        }
    }

    private sealed partial class PersistentFumoMarker : Node2D
    {
        private readonly TargetFumoSlots ownerSlots;
        private readonly NCreature target;
        private readonly Texture2D texture;
        private readonly Texture2D? secondaryTexture;
        private readonly int slot;
        private readonly float phase;
        private Sprite2D? sprite;
        private Sprite2D? secondarySprite;
        private bool showSecondary;
        private float baseSpriteScale = 1f;
        private float secondarySpriteScale = 1f;
        private float baseVisualHeight;
        private float secondaryVisualHeight;
        private float baseVisualWidth;
        private float secondaryVisualWidth;
        private float elapsed;

        public PersistentFumoMarker(TargetFumoSlots ownerSlots, NCreature target, Texture2D texture, Texture2D? secondaryTexture, int slot, bool showSecondary)
        {
            this.ownerSlots = ownerSlots;
            this.target = target;
            this.texture = texture;
            this.secondaryTexture = secondaryTexture;
            this.slot = slot;
            this.showSecondary = showSecondary;
            phase = (slot + 1) * 1.37f;
        }

        public override void _Ready()
        {
            ZIndex = 1;
            sprite = new Sprite2D
            {
                Texture = texture,
                Centered = true,
                ZIndex = 0,
                Modulate = Colors.White
            };

            if (texture.GetWidth() > 0)
            {
                baseSpriteScale = VisualWidth / texture.GetWidth();
                baseVisualWidth = texture.GetWidth() * baseSpriteScale;
                baseVisualHeight = texture.GetHeight() * baseSpriteScale;
                sprite.Scale = Vector2.One * baseSpriteScale;
                sprite.Position = new Vector2(0f, -baseVisualHeight * 0.5f);
            }

            AddChild(sprite);

            if (secondaryTexture != null)
            {
                secondarySprite = new Sprite2D
                {
                    Texture = secondaryTexture,
                    Centered = true,
                    ZIndex = 0,
                    Modulate = Colors.White,
                    Visible = showSecondary
                };

                if (secondaryTexture.GetWidth() > 0)
                {
                    secondarySpriteScale = VisualWidth / secondaryTexture.GetWidth();
                    secondaryVisualWidth = secondaryTexture.GetWidth() * secondarySpriteScale;
                    secondaryVisualHeight = secondaryTexture.GetHeight() * secondarySpriteScale;
                    secondarySprite.Scale = Vector2.One * secondarySpriteScale;
                    secondarySprite.Position = GetSecondaryBasePosition(1f);
                }

                AddChild(secondarySprite);
            }

            UpdatePosition(0f);
        }

        public override void _Process(double delta)
        {
            elapsed += (float)delta;
            if (!GodotObject.IsInstanceValid(target) || !IsInCurrentCombatVfxContainer())
            {
                this.QueueFreeSafely();
                return;
            }

            UpdatePosition((float)delta);
        }

        public override void _ExitTree()
        {
            ownerSlots.ClearSlot(slot, this);
            base._ExitTree();
        }

        public void ShowSecondary()
        {
            showSecondary = true;
            if (secondarySprite != null)
            {
                secondarySprite.Visible = true;
            }
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

                if (secondarySprite != null)
                {
                    secondarySprite.Scale = new Vector2(secondarySpriteScale * scaleX, secondarySpriteScale * scaleY);
                    secondarySprite.Position = GetSecondaryBasePosition(scaleY);
                }
            }
        }

        private Vector2 GetSecondaryBasePosition(float scaleY)
        {
            return new Vector2(baseVisualWidth * 0.5f + SecondaryGap + secondaryVisualWidth * 0.5f, -secondaryVisualHeight * scaleY * 0.5f);
        }

        private Vector2 GetAnchorPosition()
        {
            return GetTargetAnchorPosition(target, slot);
        }

        private bool IsInCurrentCombatVfxContainer()
        {
            return GetParent() == target && NCombatRoom.Instance != null;
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
        string? name = card switch
        {
            CirnoFumo_Koishi => "cirno",
            KogasaFumo_Koishi => "kogasa",
            MarisaFumo_Koishi => "marisa",
            OkinaFumo_Koishi => "okina",
            ReimuFumo_Koishi => "reimu",
            ReisenFumo_Koishi => "reisen",
            TewiFumo_Koishi => "tewi",
            YukariFumo_Koishi => "yukari",
            YuukaFumo_Koishi => "yuuka",
            FlandreFumo_Koishi => "flandre",
            LWKoishiFumo_Koishi => "lwkoishi",
            ShionFumo_Koishi => "shion",
            YoumuFumo_Koishi => "youmu",
            NueFumo_Koishi => "nue",
            YuyukoFumo_Koishi => "yuyuko",
            MinamituFumo_Koishi => "minamitu",
            KoakumaFumo_koishi => "koakuma",
            PinkKoishiFumo_Koishi => "pinkkoishi",
            MarisaMoonFumo_Koishi => "marisamoon",
            _ => null
        };

        return name != null && TryGetTexturePaths(name, out string? path, out _)
            ? path
            : null;
    }

    private static string? GetSecondaryTexturePath(CardModel card)
    {
        string? fileName = card switch
        {
            MarisaMoonFumo_Koishi => "MarisaMoonfumo_gun.png",
            _ => null
        };

        return fileName == null ? null : TextureDirectory + fileName;
    }

    private static string NormalizeTextureName(string value)
    {
        return value.Trim()
            .Replace("_", string.Empty)
            .Replace("-", string.Empty)
            .Replace("fumo", string.Empty)
            .ToLowerInvariant();
    }
}
