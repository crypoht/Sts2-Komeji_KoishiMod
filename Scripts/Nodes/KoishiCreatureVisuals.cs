using System;
using Godot;
using MegaCrit.Sts2.Core.Logging;
using MegaCrit.Sts2.Core.Nodes.Combat;

namespace KomeijiKoishi.Scripts.Nodes;

public partial class KoishiCreatureVisuals : NCreatureVisuals
{
    private const string BoomSoundPath = "res://mods/Komeiji_Koishi/sound/boom.wav";
    private string? _nextDeath;

    public bool SupportsSpecialDeaths => _specialDeathsEnabled;

    public void SetNextDeath(string effect)
    {
        _nextDeath = effect;
        Log.Info($"[KoishiDeath] Override armed: {effect}");
    }

    private string? _lastAnimation;
    private bool _specialDeathActive;
    private bool _specialDeathsEnabled;

    public override void _Ready()
    {
        base._Ready();
        _specialDeathsEnabled = SpineBody != null
            && SpineBody.HasAnimation("die_fumo")
            && SpineBody.HasAnimation("die_loop_fumo")
            && SpineBody.HasAnimation("die_boom");
        Log.Info($"[KoishiDeath] Visuals ready: specialDeaths={_specialDeathsEnabled}");
        if (!_specialDeathsEnabled)
            return;

        ProcessMode = ProcessModeEnum.Always;
        SetProcess(true);
    }

    public override void _Process(double delta)
    {
        if (!_specialDeathsEnabled)
            return;

        // Observe track zero directly: the start signal can be disconnected during
        // defeat while Spine itself continues playing its queued animations.
        var animationState = SpineBody!.GetAnimationState();
#if STS2_BETA
        using var current = animationState.GetCurrent(0);
#else
        var current = animationState.GetCurrent(0);
#endif
        if (current == null)
            return;

        string name = current.GetAnimationName();
        if (name == _lastAnimation)
            return;
        _lastAnimation = name;

        bool special = name is "die_fumo" or "die_loop_fumo" or "die_boom";
        if (special || name == "die")
            Log.Info($"[KoishiDeath] Started: {name}");
        if (special || _specialDeathActive)
        {
            // The fumo loop uses a different slot. Remove the previous attachment
            // before applying the new animation, with no crossfade back to die.
            current.SetMixDuration(0f);
            var skeleton = SpineBody.GetSkeleton();
            if (skeleton != null)
            {
                skeleton.SetSlotsToSetupPose();
                animationState.Apply(skeleton);
            }
        }
        _specialDeathActive = special;

        if (name == "die_fumo")
            animationState.AddAnimation("die_loop_fumo", current.GetAnimationEnd(), true, 0);

        if (name == "die_boom")
            PlayBoomSound(current.GetAnimationEnd() - current.GetTrackTime());

        if (name != "die")
            return;

        // Cosmetic only: never consume the shared gameplay RNG.
        string? continuation;
        if (_nextDeath is { } forced)
        {
            _nextDeath = null;
            continuation = forced == "null" ? null : "die_" + forced;
            Log.Info($"[KoishiDeath] Override consumed: {forced}");
        }
        else
        {
            int roll = Random.Shared.Next(1000);
            continuation = roll == 0 ? "die_boom" : roll <= 200 ? "die_fumo" : null;
            Log.Info($"[KoishiDeath] Random roll={roll}, continuation={continuation ?? "none"}");
        }
        if (continuation == null)
            return;

        // Explicit delays and zero mixing preserve the entire preceding animation.
        animationState.AddAnimation(continuation, current.GetAnimationEnd(), false, 0);
        Log.Info($"[KoishiDeath] Queued: {continuation}");
    }

    private void PlayBoomSound(float animationDuration)
    {
        AudioStream? stream = ResourceLoader.Load<AudioStream>(BoomSoundPath);
        if (stream == null || animationDuration <= 0f)
        {
            Log.Warn($"[KoishiDeath] Cannot play boom audio: streamLoaded={stream != null}, duration={animationDuration}");
            return;
        }

        var player = new AudioStreamPlayer
        {
            Stream = stream,
            Bus = "SFX",
            VolumeDb = Mathf.LinearToDb(1.6f),
            PitchScale = Math.Max(0.01f, (float)stream.GetLength() / animationDuration)
        };
        player.Finished += player.QueueFree;
        AddChild(player);
        player.Play();
        int bus = AudioServer.GetBusIndex(player.Bus);
        Log.Info($"[KoishiDeath] Boom audio started: length={stream.GetLength():F3}, remaining={animationDuration:F3}, pitch={player.PitchScale:F3}, bus={player.Bus}, busIndex={bus}, playing={player.Playing}");
    }
}
