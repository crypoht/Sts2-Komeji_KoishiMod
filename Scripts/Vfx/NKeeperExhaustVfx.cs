using System.Threading.Tasks;
using Godot;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.Logging;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Nodes.Combat;
using MegaCrit.Sts2.Core.Nodes.Rooms;

namespace KomeijiKoishi.Vfx;

public sealed partial class NKeeperExhaustVfx : Node2D
{
    public const float FadeInDuration = 0.18f;
    public const float FadeOutDuration = 0.25f;
    public const float DecelerationDuration = 0.45f;
    public const float HeadOffsetY = -180f;
    public const float VisualWidth = 240f;
    public const float BaseRotationSpeed = 2.5f;
    public const float RotationSpeedPerExhaustedCard = 1.25f;
    public const float RotationAcceleration = 16f;
    public const float CardFlightDuration = 0.18f;
    public const float CardVisualWidth = 43f;
    public const float CardAfterimageInterval = 0.025f;
    public const float CardAfterimageLifetime = 0.16f;
    public const float CardAfterimageAlpha = 0.35f;
    public const float MinArcHeight = 180f;
    public const float MaxArcHeight = 420f;
    public const float MaxArcSideOffset = 260f;
    private const string TexturePath = "res://mods/Komeiji_Koishi/images/qingxu/keeper.png";
    private const string CardFlyerTexturePath = "res://mods/Komeiji_Koishi/images/qingxu/keeper_2.png";

    private Vector2 startPosition;
    private Vector2 sourcePosition;
    private Sprite2D? sprite;
    private float currentRotationSpeed;
    private float targetRotationSpeed = BaseRotationSpeed;
    private bool finishing;
    private readonly RandomNumberGenerator rng = new();

    public static NKeeperExhaustVfx? Create(Creature source)
    {
        if (!ResourceLoader.Exists(TexturePath))
        {
            Log.Warn($"[KoishiKeeperExhaustVfx] Missing texture: {TexturePath}");
            return null;
        }

        NCombatRoom? room = NCombatRoom.Instance;
        NCreature? sourceNode = room?.GetCreatureNode(source);
        if (sourceNode == null)
        {
            Log.Warn("[KoishiKeeperExhaustVfx] Missing source combat node.");
            return null;
        }

        Vector2 hitboxCenter = sourceNode.Hitbox.GlobalPosition + sourceNode.Hitbox.Size * 0.5f;
        Vector2 headPosition = sourceNode.Hitbox.GlobalPosition + new Vector2(sourceNode.Hitbox.Size.X * 0.5f, HeadOffsetY);

        return new NKeeperExhaustVfx
        {
            sourcePosition = hitboxCenter,
            startPosition = headPosition
        };
    }

    public override void _Ready()
    {
        rng.Randomize();
        GlobalPosition = startPosition;

        Texture2D texture = ResourceLoader.Load<Texture2D>(TexturePath);
        sprite = new Sprite2D
        {
            Texture = texture,
            Centered = true,
            ZIndex = 135,
            Modulate = new Color(1f, 1f, 1f, 0f)
        };

        if (texture.GetWidth() > 0)
        {
            sprite.Scale = Vector2.One * (VisualWidth / texture.GetWidth());
        }

        AddChild(sprite);

        Tween fadeInTween = sprite.CreateTween();
        fadeInTween.TweenProperty(sprite, "modulate:a", 1f, FadeInDuration);
    }

    public override void _Process(double delta)
    {
        float deltaFloat = (float)delta;
        currentRotationSpeed = Mathf.MoveToward(
            currentRotationSpeed,
            targetRotationSpeed,
            RotationAcceleration * deltaFloat
        );

        if (sprite != null)
        {
            sprite.Rotation += currentRotationSpeed * deltaFloat;
        }
    }

    public void OnCardExhausted()
    {
        if (!finishing)
        {
            targetRotationSpeed += RotationSpeedPerExhaustedCard;
        }
    }

    public async Task FlyCardToKeeperAsync(CardModel card)
    {
        if (finishing)
        {
            return;
        }

        CardFlyer flyer = CardFlyer.Create(
            sourcePosition,
            GlobalPosition,
            RandomControlPoint(sourcePosition, GlobalPosition)
        );
        AddChild(flyer);

        await Cmd.Wait(CardFlightDuration, false);
    }

    public async Task FinishAsync()
    {
        if (finishing)
        {
            return;
        }

        finishing = true;
        targetRotationSpeed = 0f;

        await Cmd.Wait(DecelerationDuration, false);

        if (sprite != null)
        {
            Tween fadeOutTween = sprite.CreateTween();
            fadeOutTween.TweenProperty(sprite, "modulate:a", 0f, FadeOutDuration);
            await Cmd.Wait(FadeOutDuration, false);
        }

        this.QueueFreeSafely();
    }

    private Vector2 RandomControlPoint(Vector2 start, Vector2 end)
    {
        Vector2 midpoint = (start + end) * 0.5f;
        Vector2 direction = end - start;
        Vector2 perpendicular = direction.LengthSquared() > 0f
            ? direction.Normalized().Rotated(Mathf.Pi / 2f)
            : Vector2.Up;
        return midpoint
            + perpendicular * rng.RandfRange(-MaxArcSideOffset, MaxArcSideOffset)
            + Vector2.Up * rng.RandfRange(MinArcHeight, MaxArcHeight);
    }

    private sealed partial class CardFlyer : Node2D
    {
        private Vector2 start;
        private Vector2 control;
        private Vector2 end;
        private float elapsed;
        private float afterimageTimer;
        private Texture2D? texture;
        private Vector2 spriteScale = Vector2.One;
        private Sprite2D? sprite;

        public static CardFlyer Create(Vector2 start, Vector2 end, Vector2 control)
        {
            CardFlyer flyer = new()
            {
                start = start,
                end = end,
                control = control,
                GlobalPosition = start,
                ZIndex = 134
            };
            flyer.CreateVisual();
            return flyer;
        }

        public override void _Process(double delta)
        {
            elapsed += (float)delta;
            float t = Mathf.Clamp(elapsed / CardFlightDuration, 0f, 1f);
            float eased = 1f - Mathf.Pow(1f - t, 2f);
            GlobalPosition = QuadraticBezier(start, control, end, eased);

            if (sprite != null)
            {
                sprite.Rotation += 9.5f * (float)delta;
                sprite.Modulate = new Color(1f, 1f, 1f, 1f - Mathf.Max(0f, t - 0.8f) / 0.2f);
                afterimageTimer += (float)delta;
                if (afterimageTimer >= CardAfterimageInterval && t < 0.92f)
                {
                    afterimageTimer = 0f;
                    CreateAfterimage();
                }
            }

            if (t >= 1f)
            {
                this.QueueFreeSafely();
            }
        }

        private void CreateVisual()
        {
            if (ResourceLoader.Exists(CardFlyerTexturePath))
            {
                texture = ResourceLoader.Load<Texture2D>(CardFlyerTexturePath);
                sprite = new Sprite2D
                {
                    Texture = texture,
                    Centered = true,
                    ZIndex = 134,
                    Modulate = new Color(1f, 1f, 1f, 0.92f)
                };

                if (texture.GetWidth() > 0)
                {
                    spriteScale = Vector2.One * (CardVisualWidth / texture.GetWidth());
                    sprite.Scale = spriteScale;
                }

                AddChild(sprite);
                return;
            }

            ColorRect fallback = new()
            {
                Color = new Color(0.85f, 0.82f, 0.95f, 0.88f),
                Size = new Vector2(CardVisualWidth, CardVisualWidth * 1.4f),
                PivotOffset = new Vector2(CardVisualWidth * 0.5f, CardVisualWidth * 0.7f),
                Position = new Vector2(-CardVisualWidth * 0.5f, -CardVisualWidth * 0.7f)
            };
            AddChild(fallback);
        }

        private void CreateAfterimage()
        {
            if (texture == null || GetParent() is not Node parent)
            {
                return;
            }

            Sprite2D afterimage = new()
            {
                Texture = texture,
                Centered = true,
                GlobalPosition = GlobalPosition,
                GlobalRotation = sprite?.GlobalRotation ?? GlobalRotation,
                Scale = spriteScale * 0.9f,
                ZIndex = 133,
                Modulate = new Color(0.72f, 0.86f, 1f, CardAfterimageAlpha)
            };

            parent.AddChild(afterimage);

            Tween tween = afterimage.CreateTween();
            tween.TweenProperty(afterimage, "modulate:a", 0f, CardAfterimageLifetime);
            tween.Parallel().TweenProperty(afterimage, "scale", spriteScale * 0.65f, CardAfterimageLifetime);
            tween.TweenCallback(Callable.From(afterimage.QueueFreeSafely));
        }

        private static Vector2 QuadraticBezier(Vector2 start, Vector2 control, Vector2 end, float t)
        {
            float inverse = 1f - t;
            return inverse * inverse * start + 2f * inverse * t * control + t * t * end;
        }
    }
}
