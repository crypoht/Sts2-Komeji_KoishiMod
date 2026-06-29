using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BaseLib.Abstracts;
using BaseLib.Utils;
using Godot;
using MegaCrit.Sts2.Core.Context;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.CardPools;
using MegaCrit.Sts2.Core.Nodes;

namespace KomeijiKoishi.Cards
{
    [Pool(typeof(CurseCardPool))]
    public sealed class MoriyaDance_Koishi : CustomCardModel
    {
        private static readonly string[] DanceVideoPaths =
        {
            "res://mods/Komeiji_Koishi/Video/MoriyaDance.ogv",
            "res://mods/Komeiji_Koishi/Video/MoriyaDance_1.ogv",
            "res://mods/Komeiji_Koishi/Video/MoriyaDance_2.ogv",
            "res://mods/Komeiji_Koishi/Video/MoriyaDance_3.ogv",
            "res://mods/Komeiji_Koishi/Video/MoriyaDance_4.ogv",
            "res://mods/Komeiji_Koishi/Video/MoriyaDance_5.ogv",
            "res://mods/Komeiji_Koishi/Video/MoriyaDance_6.ogv"
        };

        private static bool _isDanceVideoPlaying;
        private static readonly Queue<string> QueuedDanceVideos = new();

        public MoriyaDance_Koishi()
            : base(-1, CardType.Curse, CardRarity.Curse, TargetType.None, true)
        {
        }

        public override string PortraitPath => KoishiImagePaths.CardPortrait(GetType());

        public override bool CanBeGeneratedByModifiers => false;

        public override int MaxUpgradeLevel => 0;

        public override IEnumerable<CardKeyword> CanonicalKeywords => new[]
        {
            CardKeyword.Eternal,
            CardKeyword.Unplayable
        };

        public override Task AfterCardDrawn(PlayerChoiceContext choiceContext, CardModel card, bool fromHandDraw)
        {
            if (card == this)
            {
                string? videoPath = base.Owner.RunState.Rng.Shuffle.NextItem(DanceVideoPaths);
                if (videoPath != null && (KoishiModConfig.PlayMoriyaDanceForAllPlayers || LocalContext.IsMine(this)))
                {
                    PlayDanceVideo(videoPath);
                }
            }

            return Task.CompletedTask;
        }

        private static void PlayDanceVideo(string videoPath)
        {
            if (_isDanceVideoPlaying)
            {
                QueuedDanceVideos.Enqueue(videoPath);
                return;
            }

            if (!ResourceLoader.Exists(videoPath))
            {
                return;
            }

            VideoStream? stream = ResourceLoader.Load<VideoStream>(videoPath);
            if (stream == null)
            {
                return;
            }

            Control? parent = NRun.Instance?.GlobalUi;
            if (parent == null)
            {
                return;
            }

            _isDanceVideoPlaying = true;

            Control overlay = new Control
            {
                MouseFilter = Control.MouseFilterEnum.Ignore,
                AnchorLeft = 0f,
                AnchorTop = 0f,
                AnchorRight = 1f,
                AnchorBottom = 1f,
                OffsetLeft = 0f,
                OffsetTop = 0f,
                OffsetRight = 0f,
                OffsetBottom = 0f
            };

            VideoStreamPlayer player = new VideoStreamPlayer
            {
                Stream = stream,
                Autoplay = false,
                Expand = true,
                CustomMinimumSize = new Vector2(960f, 540f),
                AnchorLeft = 0.5f,
                AnchorTop = 0.5f,
                AnchorRight = 0.5f,
                AnchorBottom = 0.5f,
                OffsetLeft = -480f,
                OffsetTop = -270f,
                OffsetRight = 480f,
                OffsetBottom = 270f
            };

            player.Finished += () =>
            {
                overlay.QueueFree();
                _isDanceVideoPlaying = false;

                if (QueuedDanceVideos.Count > 0)
                {
                    string nextVideoPath = QueuedDanceVideos.Dequeue();
                    PlayDanceVideo(nextVideoPath);
                }
            };
            overlay.AddChildSafely(player);
            parent.AddChildSafely(overlay);
            player.Play();
        }
    }
}
