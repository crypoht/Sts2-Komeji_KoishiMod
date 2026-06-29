using Godot;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.Logging;
using MegaCrit.Sts2.Core.Nodes.Combat;
using MegaCrit.Sts2.Core.Nodes.Rooms;

namespace KomeijiKoishi.Vfx;

public sealed partial class NDanmakuProjectileVfx : Node2D
{
    public const float DefaultDuration = 0.2f;
    public const float DefaultOvershootDistance = 1000f;
    private const float DefaultVisualWidth = 80f;

    private Vector2 startPosition;
    private Vector2 endPosition;
    private string texturePath = string.Empty;
    private float duration = DefaultDuration;
    private float visualWidth = DefaultVisualWidth;

    public static NDanmakuProjectileVfx? Create(
        Creature source,
        Creature target,
        string texturePath,
        float duration = DefaultDuration,
        float visualWidth = DefaultVisualWidth)
    {
        if (!ResourceLoader.Exists(texturePath))
        {
            Log.Warn($"[KoishiDanmakuProjectile] Missing texture: {texturePath}");
            return null;
        }

        NCombatRoom? room = NCombatRoom.Instance;
        NCreature? sourceNode = room?.GetCreatureNode(source);
        NCreature? targetNode = room?.GetCreatureNode(target);
        if (sourceNode == null || targetNode == null)
        {
            Log.Warn($"[KoishiDanmakuProjectile] Missing combat node. sourceNode={sourceNode != null}, targetNode={targetNode != null}");
            return null;
        }

        return new NDanmakuProjectileVfx
        {
            startPosition = sourceNode.VfxSpawnPosition,
            endPosition = targetNode.VfxSpawnPosition,
            texturePath = texturePath,
            duration = duration,
            visualWidth = visualWidth
        };
    }

    public override void _Ready()
    {
        GlobalPosition = startPosition;

        Texture2D texture = ResourceLoader.Load<Texture2D>(texturePath);
        Sprite2D sprite = new Sprite2D
        {
            Texture = texture,
            Centered = true,
            ZIndex = 100
        };

        if (texture.GetWidth() > 0)
        {
            sprite.Scale = Vector2.One * (visualWidth / texture.GetWidth());
        }

        Vector2 direction = endPosition - startPosition;
        if (direction.LengthSquared() > 0f)
        {
            sprite.Rotation = direction.Angle();
        }

        AddChild(sprite);

        Tween tween = CreateTween();
        tween.SetEase(Tween.EaseType.Out);
        tween.SetTrans(Tween.TransitionType.Quad);
        tween.TweenProperty(this, "global_position", GetOvershootEndPosition(), duration);
        tween.TweenProperty(sprite, "modulate:a", 0.0f, 0.05f);
        tween.TweenCallback(Callable.From(this.QueueFreeSafely));
    }

    private Vector2 GetOvershootEndPosition()
    {
        Vector2 direction = endPosition - startPosition;
        if (direction.LengthSquared() <= 0f)
        {
            return endPosition;
        }

        return endPosition + direction.Normalized() * DefaultOvershootDistance;
    }
}
