using Godot;
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
    public const float FadeOutStart = 0.82f;

    private Vector2 startPosition;
    private Vector2 endPosition;
    private Vector2 velocity;
    private float gravity;
    private float elapsed;
    private string texturePath = string.Empty;
    private Sprite2D? sprite;

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

        Vector2 start = GetHeadPosition(sourceNode);
        Vector2 end = GetHeadPosition(targetNode);
        return new NGiftYouFumoVfx
        {
            startPosition = start,
            endPosition = end,
            texturePath = path
        };
    }

    public override void _Ready()
    {
        GlobalPosition = startPosition;
        SolveGravityArc();

        Texture2D texture = ResourceLoader.Load<Texture2D>(texturePath);
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
            if (t >= FadeOutStart)
            {
                float fadeT = (t - FadeOutStart) / (1f - FadeOutStart);
                sprite.Modulate = new Color(1f, 1f, 1f, 1f - fadeT);
            }
        }

        if (t >= 1f)
        {
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
