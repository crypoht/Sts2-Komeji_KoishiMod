using System.Collections.Generic;
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

public sealed partial class NUndergroundRoseCircleVfx : Node2D
{
    public const float Radius = 210f;
    public const float SpawnInterval = 0.02f;
    public const float AngleStepDegrees = 5f;
    public const int MaxSpritesPerColor = 7;
    public const float VisualWidth = 126f;
    public const float FadeOutDuration = 0.14f;
    public const float FinalFadeInterval = 0.025f;
    private const float RedStartAngleDegrees = -45f;
    private const float BlueStartAngleDegrees = 135f;
    private const string TextureDirectory = "res://mods/Komeiji_Koishi/images/qingxu/";
    private const string RedTexturePath = TextureDirectory + "rose_red.png";
    private const string BlueTexturePath = TextureDirectory + "rose_blue.png";

    private readonly Queue<Sprite2D> redSprites = new();
    private readonly Queue<Sprite2D> blueSprites = new();
    private Vector2 centerPosition;
    private Texture2D? redTexture;
    private Texture2D? blueTexture;
    private CancellationTokenSource? cts;

    public static NUndergroundRoseCircleVfx? Create(Creature source)
    {
        if (!ResourceLoader.Exists(RedTexturePath))
        {
            Log.Warn($"[KoishiUndergroundRoseCircleVfx] Missing texture: {RedTexturePath}");
            return null;
        }

        if (!ResourceLoader.Exists(BlueTexturePath))
        {
            Log.Warn($"[KoishiUndergroundRoseCircleVfx] Missing texture: {BlueTexturePath}");
            return null;
        }

        NCombatRoom? room = NCombatRoom.Instance;
        NCreature? sourceNode = room?.GetCreatureNode(source);
        if (sourceNode == null)
        {
            Log.Warn("[KoishiUndergroundRoseCircleVfx] Missing source combat node.");
            return null;
        }

        return new NUndergroundRoseCircleVfx
        {
            centerPosition = sourceNode.Hitbox.GlobalPosition + sourceNode.Hitbox.Size * 0.5f
        };
    }

    public override void _Ready()
    {
        GlobalPosition = centerPosition;
        redTexture = ResourceLoader.Load<Texture2D>(RedTexturePath);
        blueTexture = ResourceLoader.Load<Texture2D>(BlueTexturePath);
        cts = new CancellationTokenSource();
        TaskHelper.RunSafely(PlayAsync(cts.Token));
    }

    public override void _ExitTree()
    {
        cts?.Cancel();
        cts?.Dispose();
        cts = null;
    }

    private async Task PlayAsync(CancellationToken token)
    {
        int totalSteps = Mathf.RoundToInt(360f / AngleStepDegrees);
        for (int step = 0; step <= totalSteps; step++)
        {
            float angleOffset = step * AngleStepDegrees;
            SpawnRose(redTexture, RedStartAngleDegrees + angleOffset, redSprites);
            SpawnRose(blueTexture, BlueStartAngleDegrees + angleOffset, blueSprites);
            await Cmd.Wait(SpawnInterval, token, false);
        }

        await FadeRemainingAsync(redSprites, token);
        await FadeRemainingAsync(blueSprites, token);
        this.QueueFreeSafely();
    }

    private void SpawnRose(Texture2D? texture, float angleDegrees, Queue<Sprite2D> queue)
    {
        if (texture == null)
        {
            return;
        }

        Sprite2D sprite = new()
        {
            Texture = texture,
            Centered = true,
            Position = OffsetFromAngle(angleDegrees),
            GlobalRotation = Mathf.DegToRad(angleDegrees) + Mathf.Pi / 2f,
            ZIndex = 126,
            Modulate = new Color(1f, 1f, 1f, 0.94f)
        };

        if (texture.GetWidth() > 0)
        {
            sprite.Scale = Vector2.One * (VisualWidth / texture.GetWidth());
        }

        AddChild(sprite);
        queue.Enqueue(sprite);

        if (queue.Count > MaxSpritesPerColor)
        {
            FadeAndFree(queue.Dequeue());
        }
    }

    private static Vector2 OffsetFromAngle(float angleDegrees)
    {
        float radians = Mathf.DegToRad(angleDegrees);
        return new Vector2(Mathf.Cos(radians), Mathf.Sin(radians)) * Radius;
    }

    private static void FadeAndFree(Sprite2D sprite)
    {
        if (!GodotObject.IsInstanceValid(sprite))
        {
            return;
        }

        Tween tween = sprite.CreateTween();
        tween.TweenProperty(sprite, "modulate:a", 0f, FadeOutDuration);
        tween.Parallel().TweenProperty(sprite, "scale", sprite.Scale * 0.72f, FadeOutDuration);
        tween.TweenCallback(Callable.From(sprite.QueueFreeSafely));
    }

    private static async Task FadeRemainingAsync(Queue<Sprite2D> queue, CancellationToken token)
    {
        while (queue.Count > 0)
        {
            FadeAndFree(queue.Dequeue());
            await Cmd.Wait(FinalFadeInterval, token, false);
        }

        await Cmd.Wait(FadeOutDuration, token, false);
    }
}
