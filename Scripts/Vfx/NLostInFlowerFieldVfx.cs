using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Godot;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.Logging;
using MegaCrit.Sts2.Core.Nodes.Combat;
using MegaCrit.Sts2.Core.Nodes.Rooms;

namespace KomeijiKoishi.Vfx;

public sealed partial class NLostInFlowerFieldVfx : Node2D
{
    public const int FlowerCount = 80;
    public const float FloorOffsetY = 135f;
    public const float FloorRadiusX = 500f;
    public const float FloorRadiusY = 170f;
    public const float FloorSkewX = -0.65f;
    public const float PlayerClearPadding = 20f;
    public const float MinVisualWidth = 42f;
    public const float MaxVisualWidth = 72f;
    public const float SpreadDuration = 0.85f;
    public const float FadeInDuration = 0.16f;
    public const float HoldDuration = 2f;
    public const float FadeOutDuration = 3f;
    public const int FlowerKinds = 9;
    public const int GroundZIndex = 12;
    private const string TextureDirectory = "res://mods/Komeiji_Koishi/images/qingxu/";

    private readonly List<Texture2D> textures = new();
    private readonly List<Sprite2D> sprites = new();
    private readonly List<ClearArea> playerClearAreas = new();
    private readonly RandomNumberGenerator rng = new();
    private Vector2 floorCenter;
    private CancellationTokenSource? cts;

    public static NLostInFlowerFieldVfx? Create(Creature source)
    {
        NCombatRoom? room = NCombatRoom.Instance;
        NCreature? sourceNode = room?.GetCreatureNode(source);
        if (room == null || sourceNode == null || source.CombatState == null)
        {
            Log.Warn("[KoishiLostInFlowerFieldVfx] Missing source combat node.");
            return null;
        }

        NLostInFlowerFieldVfx vfx = new()
        {
            floorCenter = sourceNode.VfxSpawnPosition + new Vector2(0f, FloorOffsetY)
        };

        foreach (Creature creature in source.CombatState.GetCreaturesOnSide(source.Side).Where(creature => creature.IsPlayer))
        {
            NCreature? creatureNode = room.GetCreatureNode(creature);
            if (creatureNode == null)
            {
                continue;
            }

            Vector2 hitboxCenter = creatureNode.Hitbox.GlobalPosition + creatureNode.Hitbox.Size * 0.5f;
            Vector2 hitboxRadius = creatureNode.Hitbox.Size * 0.5f + Vector2.One * PlayerClearPadding;
            vfx.playerClearAreas.Add(new ClearArea(hitboxCenter - vfx.floorCenter, hitboxRadius));
        }

        return vfx;
    }

    public override void _Ready()
    {
        rng.Randomize();
        GlobalPosition = floorCenter;

        for (int i = 1; i <= FlowerKinds; i++)
        {
            string texturePath = TextureDirectory + $"flower_{i}.png";
            if (!ResourceLoader.Exists(texturePath))
            {
                Log.Warn($"[KoishiLostInFlowerFieldVfx] Missing texture: {texturePath}");
                continue;
            }

            textures.Add(ResourceLoader.Load<Texture2D>(texturePath));
        }

        if (textures.Count == 0)
        {
            QueueFree();
            return;
        }

        List<FlowerPlacement> placements = CreatePlacements();
        for (int i = 0; i < placements.Count; i++)
        {
            CreateFlower(i, placements[i]);
        }

        cts = new CancellationTokenSource();
        TaskHelper.RunSafely(FinishAsync(cts.Token));
    }

    public override void _ExitTree()
    {
        cts?.Cancel();
        cts?.Dispose();
        cts = null;
    }

    private List<FlowerPlacement> CreatePlacements()
    {
        List<FlowerPlacement> placements = new();
        for (int i = 0; i < FlowerCount; i++)
        {
            Vector2 position = RandomFloorPoint();
            float normalizedDistance = Mathf.Clamp(position.Length() / FloorRadiusX, 0f, 1f);
            placements.Add(new FlowerPlacement(position, normalizedDistance));
        }

        return placements.OrderBy(placement => placement.NormalizedDistance).ToList();
    }

    private void CreateFlower(int index, FlowerPlacement placement)
    {
        Texture2D texture = textures[rng.RandiRange(0, textures.Count - 1)];
        Sprite2D sprite = new()
        {
            Texture = texture,
            Centered = true,
            Position = placement.Position,
            Rotation = rng.RandfRange(-0.35f, 0.35f),
            ZIndex = GroundZIndex + index,
            Modulate = new Color(1f, 1f, 1f, 0f)
        };

        if (texture.GetWidth() > 0)
        {
            float visualWidth = rng.RandfRange(MinVisualWidth, MaxVisualWidth);
            sprite.Scale = Vector2.One * (visualWidth / texture.GetWidth());
        }

        AddChild(sprite);
        sprites.Add(sprite);

        float targetAlpha = rng.RandfRange(0.72f, 0.95f);
        float delay = placement.NormalizedDistance * SpreadDuration;
        Tween tween = sprite.CreateTween();
        tween.TweenProperty(sprite, "modulate:a", targetAlpha, FadeInDuration)
            .SetDelay(delay);
        tween.Parallel().TweenProperty(sprite, "scale", sprite.Scale, FadeInDuration)
            .From(sprite.Scale * 0.45f)
            .SetDelay(delay)
            .SetEase(Tween.EaseType.Out)
            .SetTrans(Tween.TransitionType.Back);
    }

    private Vector2 RandomFloorPoint()
    {
        for (int i = 0; i < 12; i++)
        {
            float angle = rng.RandfRange(0f, Mathf.Tau);
            float radius = Mathf.Sqrt(rng.Randf());
            float y = Mathf.Sin(angle) * FloorRadiusY * radius;
            float x = Mathf.Cos(angle) * FloorRadiusX * radius + y * FloorSkewX;
            Vector2 position = new(x, y);
            if (!IsInsidePlayerClearArea(position))
            {
                return position;
            }
        }

        return new Vector2(rng.RandfRange(-FloorRadiusX, FloorRadiusX), FloorRadiusY * rng.RandfRange(0.35f, 1f));
    }

    private bool IsInsidePlayerClearArea(Vector2 position)
    {
        foreach (ClearArea clearArea in playerClearAreas)
        {
            if (clearArea.Radius.X <= 0f || clearArea.Radius.Y <= 0f)
            {
                continue;
            }

            Vector2 localPosition = position - clearArea.Center;
            float normalizedX = localPosition.X / clearArea.Radius.X;
            float normalizedY = localPosition.Y / clearArea.Radius.Y;
            if (normalizedX * normalizedX + normalizedY * normalizedY < 1f)
            {
                return true;
            }
        }

        return false;
    }

    private async Task FinishAsync(CancellationToken token)
    {
        await Cmd.Wait(SpreadDuration + FadeInDuration + HoldDuration, token, false);

        Tween fadeTween = CreateTween();
        fadeTween.SetParallel(true);
        foreach (Sprite2D sprite in sprites)
        {
            if (!GodotObject.IsInstanceValid(sprite))
            {
                continue;
            }

            fadeTween.TweenProperty(sprite, "modulate:a", 0f, FadeOutDuration);
            fadeTween.TweenProperty(sprite, "scale", sprite.Scale * 0.82f, FadeOutDuration);
        }

        await Cmd.Wait(FadeOutDuration, token, false);
        this.QueueFreeSafely();
    }

    private readonly record struct FlowerPlacement(Vector2 Position, float NormalizedDistance);

    private readonly record struct ClearArea(Vector2 Center, Vector2 Radius);
}
