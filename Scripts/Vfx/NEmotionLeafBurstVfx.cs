using System.Collections.Generic;
using Godot;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.Logging;
using MegaCrit.Sts2.Core.Nodes.Combat;
using MegaCrit.Sts2.Core.Nodes.Rooms;

namespace KomeijiKoishi.Vfx;

public sealed partial class NEmotionLeafBurstVfx : Node2D
{
    public const int LeafCount = 14;
    public const float BurstDuration = 0.28f;
    public const float FallDuration = 0.55f;
    public const float FadeDuration = 0.45f;
    public const float MinBurstDistance = 150f;
    public const float MaxBurstDistance = 230f;
    public const float FallDistance = 200f;
    public const float MinVisualWidth = 24f;
    public const float MaxVisualWidth = 38f;
    public const float MinDriftAmplitude = 12f;
    public const float MaxDriftAmplitude = 28f;
    public const float MinDriftSpeed = 6f;
    public const float MaxDriftSpeed = 10f;
    public const float MinRotationSpeed = -7f;
    public const float MaxRotationSpeed = 7f;
    private const string TextureDirectory = "res://mods/Komeiji_Koishi/images/qingxu/";

    private readonly List<LeafState> leaves = new();
    private string texturePath = string.Empty;
    private Vector2 startPosition;
    private readonly RandomNumberGenerator rng = new();

    public static NEmotionLeafBurstVfx? Create(Creature source, string imageName)
    {
        string texturePath = TextureDirectory + imageName + ".png";
        if (!ResourceLoader.Exists(texturePath))
        {
            Log.Warn($"[KoishiEmotionLeafBurst] Missing texture: {texturePath}");
            return null;
        }

        NCombatRoom? room = NCombatRoom.Instance;
        NCreature? sourceNode = room?.GetCreatureNode(source);
        if (sourceNode == null)
        {
            Log.Warn("[KoishiEmotionLeafBurst] Missing source combat node.");
            return null;
        }

        return new NEmotionLeafBurstVfx
        {
            startPosition = sourceNode.VfxSpawnPosition,
            texturePath = texturePath
        };
    }

    public override void _Ready()
    {
        rng.Randomize();
        GlobalPosition = startPosition;

        Texture2D texture = ResourceLoader.Load<Texture2D>(texturePath);
        for (int i = 0; i < LeafCount; i++)
        {
            CreateLeaf(texture);
        }

        Tween cleanupTween = CreateTween();
        cleanupTween.TweenInterval(BurstDuration + FallDuration + 0.05f);
        cleanupTween.TweenCallback(Callable.From(this.QueueFreeSafely));
    }

    public override void _Process(double delta)
    {
        float deltaFloat = (float)delta;
        foreach (LeafState leaf in leaves)
        {
            leaf.Time += deltaFloat;
            leaf.Sprite.Position = new Vector2(
                Mathf.Sin(leaf.Time * leaf.DriftSpeed + leaf.Phase) * leaf.DriftAmplitude,
                0f
            );
            leaf.Sprite.Rotation += leaf.RotationSpeed * deltaFloat;
        }
    }

    private void CreateLeaf(Texture2D texture)
    {
        Node2D holder = new Node2D
        {
            GlobalPosition = startPosition
        };

        Sprite2D sprite = new Sprite2D
        {
            Texture = texture,
            Centered = true,
            ZIndex = 130,
            Rotation = rng.RandfRange(0f, Mathf.Tau)
        };

        if (texture.GetWidth() > 0)
        {
            float visualWidth = rng.RandfRange(MinVisualWidth, MaxVisualWidth);
            sprite.Scale = Vector2.One * (visualWidth / texture.GetWidth());
        }

        holder.AddChild(sprite);
        AddChild(holder);

        Vector2 burstEnd = startPosition + RandomBurstOffset();
        Vector2 fallEnd = burstEnd + new Vector2(0f, FallDistance);

        Tween tween = holder.CreateTween();
        tween.SetEase(Tween.EaseType.Out);
        tween.SetTrans(Tween.TransitionType.Quad);
        tween.TweenProperty(holder, "global_position", burstEnd, BurstDuration);
        tween.SetEase(Tween.EaseType.InOut);
        tween.SetTrans(Tween.TransitionType.Linear);
        tween.TweenProperty(holder, "global_position", fallEnd, FallDuration);

        Tween fadeTween = sprite.CreateTween();
        fadeTween.TweenInterval(BurstDuration + (FallDuration - FadeDuration));
        fadeTween.TweenProperty(sprite, "modulate:a", 0f, FadeDuration);

        leaves.Add(new LeafState(
            sprite,
            rng.RandfRange(MinDriftAmplitude, MaxDriftAmplitude),
            rng.RandfRange(MinDriftSpeed, MaxDriftSpeed),
            rng.RandfRange(0f, Mathf.Tau),
            rng.RandfRange(MinRotationSpeed, MaxRotationSpeed)
        ));
    }

    private Vector2 RandomBurstOffset()
    {
        float angle = rng.RandfRange(0f, Mathf.Tau);
        float distance = rng.RandfRange(MinBurstDistance, MaxBurstDistance);
        return new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)) * distance;
    }

    private sealed class LeafState
    {
        public LeafState(Sprite2D sprite, float driftAmplitude, float driftSpeed, float phase, float rotationSpeed)
        {
            Sprite = sprite;
            DriftAmplitude = driftAmplitude;
            DriftSpeed = driftSpeed;
            Phase = phase;
            RotationSpeed = rotationSpeed;
        }

        public Sprite2D Sprite { get; }
        public float DriftAmplitude { get; }
        public float DriftSpeed { get; }
        public float Phase { get; }
        public float RotationSpeed { get; }
        public float Time { get; set; }
    }
}
