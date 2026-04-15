using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BaseLib.Utils;
using BaseLib.Abstracts; 
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.Entities.Creatures; 
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.ValueProps; 
using KomeijiKoishi.Enums;
using KomeijiKoishi.Cards.Danmaku; 

namespace KomeijiKoishi.Powers
{
    public sealed class DanmakuArtPower : CustomPowerModel
    {
        public override PowerType Type => PowerType.Buff;
        public override PowerStackType StackType => PowerStackType.Counter;
        
        public override string? CustomPackedIconPath => $"res://mods/Komeiji_Koishi/images/powers/{GetType().Name}.png";

        public override string? CustomBigIconPath => $"res://mods/Komeiji_Koishi/images/powers/{GetType().Name}.png";
        public override async Task AfterCardPlayed(PlayerChoiceContext context, CardPlay cardPlay)
        {
            try
            {
                if (cardPlay?.Card != null && cardPlay.Card.Owner == base.Owner.Player)
                {
                    bool isDanmaku = cardPlay.Card.Tags != null && cardPlay.Card.Tags.Contains(KoishiTags.Danmaku);
                    if (isDanmaku)
                    {
                        this.Flash();
                        
                        await CreatureCmd.GainBlock(base.Owner, base.Amount, ValueProp.Unpowered, null, false);
                    }
                }
            }
            catch (Exception e)
            {
                MegaCrit.Sts2.Core.Logging.Log.Error($"[DanmakuArtPower] 拦截报错: {e.Message}");
            }
        }
    }
}