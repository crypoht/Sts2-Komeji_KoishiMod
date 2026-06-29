using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Godot;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.Logging;
using MegaCrit.Sts2.Core.Nodes.GodotExtensions;
using MegaCrit.Sts2.Core.Nodes.Combat;
using MegaCrit.Sts2.Core.Nodes.Rooms;
using KomeijiKoishi.Cards;

namespace KomeijiKoishi.Vfx;

public sealed partial class NYinYangGhostGodOrbAttackVfx : Node2D
{
    public const int FanOrbCount = 7;
    public const float FanSpreadDuration = 0.3f;
    public const float FanHoldDuration = 0.1f;
    public const float FanReturnDurationPerOrb = 0.15f;
    public const float FinalProjectileDuration = 0.25f;
    public const float RotationDuration = 0.6f;
    public const float FanRadius = 160f;
    public const float SmallOrbVisualWidth = 80f;
    public const float FinalOrbVisualWidth = 300f;
    public const float FinalProjectileOvershootDistance = 1000f;
    public const float BackgroundDimAlpha = 0.5f;
    public const float BackgroundDimFadeInDuration = 0.1f;
    public const float BackgroundDimFadeOutDuration = 0.2f;
    public const float BackgroundDimExtraMargin = 512f;
    public const string GatherSoundPath = "res://mods/Komeiji_Koishi/sound/se_ch00.wav";
    public const string FireSoundPath = "res://mods/Komeiji_Koishi/sound/se_damage00.wav";

    private static readonly string[] FanOrbImageNames =
    {
        "red",
        "pink",
        "cyan",
        "blue",
        "green",
        "yellow",
        "black"
    };

    private static readonly float[] FanOrbAngles =
    {
        0f,
        45f,
        90f,
        135f,
        180f,
        225f,
        315f
    };

    private static readonly int[] FanOrbGatherOrder =
    {
        6,
        0,
        1,
        2,
        3,
        4,
        5
    };

    private Vector2 startPosition;
    private Vector2 endPosition;
    private readonly List<Sprite2D> fanSprites = new();
    private Sprite2D? finalSprite;
    private ColorRect? dimOverlay;
    private CancellationTokenSource? cts;

    public static NYinYangGhostGodOrbAttackVfx? Create(Creature source, Creature target)
    {
        NCombatRoom? room = NCombatRoom.Instance;
        NCreature? sourceNode = room?.GetCreatureNode(source);
        NCreature? targetNode = room?.GetCreatureNode(target);
        if (sourceNode == null || targetNode == null)
        {
            Log.Warn($"[KoishiYinYangGhostGodOrbVfx] Missing combat node. sourceNode={sourceNode != null}, targetNode={targetNode != null}");
            return null;
        }

        string finalTexturePath = KoishiImagePaths.DanmakuProjectile("YinYangGhostGodOrb");
        if (!ResourceLoader.Exists(finalTexturePath))
        {
            Log.Warn($"[KoishiYinYangGhostGodOrbVfx] Missing texture: {finalTexturePath}");
            return null;
        }

        foreach (string imageName in FanOrbImageNames)
        {
            string texturePath = KoishiImagePaths.DanmakuProjectile(imageName);
            if (!ResourceLoader.Exists(texturePath))
            {
                Log.Warn($"[KoishiYinYangGhostGodOrbVfx] Missing texture: {texturePath}");
                return null;
            }
        }

        return new NYinYangGhostGodOrbAttackVfx
        {
            startPosition = sourceNode.VfxSpawnPosition,
            endPosition = targetNode.VfxSpawnPosition
        };
    }

    public override void _Ready()
    {
        GlobalPosition = startPosition;
        CreateDimOverlay();
        CreateFanSprites();
        CreateFinalSprite();
        cts = new CancellationTokenSource();
        TaskHelper.RunSafely(PlaySequence(cts.Token));
    }

    public override void _ExitTree()
    {
        cts?.Cancel();
        cts?.Dispose();
        cts = null;
    }

    public override void _Process(double delta)
    {
        UpdateDimOverlayRect();
    }

    private void CreateDimOverlay()
    {
        dimOverlay = new ColorRect
        {
            Name = "KoishiYinYangGhostGodOrbDimOverlay",
            Color = new Color(0f, 0f, 0f, BackgroundDimAlpha),
            MouseFilter = Control.MouseFilterEnum.Ignore,
            Modulate = new Color(1f, 1f, 1f, 0f),
            ZIndex = 0
        };
        dimOverlay.SetAnchorsPreset(Control.LayoutPreset.FullRect);

        NCombatRoom.Instance?.BackCombatVfxContainer.AddChildSafely(dimOverlay);
        UpdateDimOverlayRect();

        Tween tween = dimOverlay.CreateTween();
        tween.TweenProperty(dimOverlay, "modulate:a", 1f, BackgroundDimFadeInDuration);
    }

    private void UpdateDimOverlayRect()
    {
        if (dimOverlay == null)
        {
            return;
        }

        Vector2 margin = Vector2.One * BackgroundDimExtraMargin;
        dimOverlay.GlobalPosition = -margin;
        dimOverlay.Size = GetViewportRect().Size + margin * 2f;
    }

    private void CreateFanSprites()
    {
        for (int i = 0; i < FanOrbImageNames.Length; i++)
        {
            string texturePath = KoishiImagePaths.DanmakuProjectile(FanOrbImageNames[i]);
            Texture2D texture = ResourceLoader.Load<Texture2D>(texturePath);
            Sprite2D sprite = CreateSprite(texture, SmallOrbVisualWidth);
            sprite.GlobalPosition = startPosition;
            AddChild(sprite);
            fanSprites.Add(sprite);
        }
    }

    private void CreateFinalSprite()
    {
        string texturePath = KoishiImagePaths.DanmakuProjectile("YinYangGhostGodOrb");
        Texture2D texture = ResourceLoader.Load<Texture2D>(texturePath);
        finalSprite = CreateSprite(texture, FinalOrbVisualWidth);
        finalSprite.GlobalPosition = startPosition;
        finalSprite.Visible = false;
        AddChild(finalSprite);
    }

    private static Sprite2D CreateSprite(Texture2D texture, float visualWidth)
    {
        Sprite2D sprite = new Sprite2D
        {
            Texture = texture,
            Centered = true,
            ZIndex = 120
        };

        if (texture.GetWidth() > 0)
        {
            sprite.Scale = Vector2.One * (visualWidth / texture.GetWidth());
        }

        return sprite;
    }

    private async Task PlaySequence(CancellationToken token)
    {
        Tween spreadTween = CreateTween();
        spreadTween.SetEase(Tween.EaseType.Out);
        spreadTween.SetTrans(Tween.TransitionType.Quad);

        for (int i = 0; i < fanSprites.Count; i++)
        {
            Vector2 offset = GetFanOffset(i);
            Tween tweener = i == 0 ? spreadTween : spreadTween.Parallel();
            tweener.TweenProperty(fanSprites[i], "global_position", startPosition + offset, FanSpreadDuration);
        }

        await Cmd.Wait(FanSpreadDuration + FanHoldDuration, token, false);

        PlaySound(GatherSoundPath);

        for (int i = 0; i < FanOrbGatherOrder.Length; i++)
        {
            int spriteIndex = FanOrbGatherOrder[i];
            Tween returnTween = CreateTween();
            returnTween.SetEase(Tween.EaseType.In);
            returnTween.SetTrans(Tween.TransitionType.Quad);
            returnTween.TweenProperty(fanSprites[spriteIndex], "global_position", startPosition, FanReturnDurationPerOrb);
            await Cmd.Wait(FanReturnDurationPerOrb, token, false);
        }

        foreach (Sprite2D sprite in fanSprites)
        {
            sprite.Visible = false;
        }

        if (finalSprite == null)
        {
            QueueFree();
            return;
        }

        finalSprite.GlobalPosition = startPosition;
        finalSprite.Visible = true;
        PlaySound(FireSoundPath);

        Tween finalTween = CreateTween();
        finalTween.SetEase(Tween.EaseType.Out);
        finalTween.SetTrans(Tween.TransitionType.Quad);
        finalTween.TweenProperty(finalSprite, "global_position", GetFinalProjectileEndPosition(), FinalProjectileDuration);
        finalTween.Parallel().TweenProperty(finalSprite, "rotation", finalSprite.Rotation + Mathf.Tau * (FinalProjectileDuration / RotationDuration), FinalProjectileDuration);

        await Cmd.Wait(FinalProjectileDuration, token, false);

        await FadeOutDimOverlay(token);

        QueueFree();
    }

    private static Vector2 GetFanOffset(int index)
    {
        float angleDegrees = FanOrbAngles[index];
        float radians = Mathf.DegToRad(angleDegrees);
        return new Vector2(Mathf.Cos(radians), -Mathf.Sin(radians)) * FanRadius;
    }

    private Vector2 GetFinalProjectileEndPosition()
    {
        Vector2 direction = endPosition - startPosition;
        if (direction.LengthSquared() <= 0f)
        {
            return endPosition;
        }

        return endPosition + direction.Normalized() * FinalProjectileOvershootDistance;
    }

    private async Task FadeOutDimOverlay(CancellationToken token)
    {
        if (dimOverlay == null)
        {
            return;
        }

        Tween tween = dimOverlay.CreateTween();
        tween.TweenProperty(dimOverlay, "modulate:a", 0f, BackgroundDimFadeOutDuration);
        await Cmd.Wait(BackgroundDimFadeOutDuration, token, false);
        dimOverlay.QueueFree();
        dimOverlay = null;
    }

    private void PlaySound(string soundPath)
    {
        if (!ResourceLoader.Exists(soundPath))
        {
            Log.Warn($"[KoishiYinYangGhostGodOrbVfx] Missing sound: {soundPath}");
            return;
        }

        AudioStream? stream = ResourceLoader.Load<AudioStream>(soundPath);
        if (stream == null)
        {
            Log.Warn($"[KoishiYinYangGhostGodOrbVfx] Failed to load sound: {soundPath}");
            return;
        }

        AudioStreamPlayer player = new AudioStreamPlayer
        {
            Stream = stream,
            Bus = "SFX"
        };
        player.Finished += player.QueueFree;
        AddChild(player);
        player.Play();
    }
}
