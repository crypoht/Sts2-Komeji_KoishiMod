using System;
using System.Linq;
using System.Threading.Tasks;
using BaseLib.Abstracts;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.Nodes.CommonUi; 
using KomeijiKoishi.Enums;
using KomeijiKoishi.Cards.Danmaku; 
using MegaCrit.Sts2.Core.Entities.Powers; 
using MegaCrit.Sts2.Core.Models; 
using MegaCrit.Sts2.Core.Models.Cards; 
using MegaCrit.Sts2.Core.GameActions.Multiplayer; 

namespace KomeijiKoishi.Powers
{
    public sealed class RecognizedGeniusPower : CustomPowerModel
    {
        public override PowerType Type => PowerType.Buff;
        public override PowerStackType StackType => PowerStackType.Counter;
        
        public override string? CustomPackedIconPath => $"res://mods/Komeiji_Koishi/images/powers/{GetType().Name}.png";

        public override Task AfterCardGeneratedForCombat(CardModel card, bool addedByPlayer)
        {
            try
            {
                if (card != null && card.Owner == base.Owner.Player)
                {
                    bool isDanmaku = (card.Tags != null && card.Tags.Contains(KoishiTags.Danmaku)) || 
                                     DanmakuPool.Pool.Any(c => c.GetType() == card.GetType());

                    if (isDanmaku)
                    {
                        if (card.IsUpgradable && !card.IsUpgraded)
                        {
                            this.Flash(); 
                            
                            CardCmd.Upgrade(card, CardPreviewStyle.None);
                        }
                    }
                }
            }
            catch (Exception e)
            {
                MegaCrit.Sts2.Core.Logging.Log.Error($"[RecognizedGeniusPower] 拦截报错: {e.Message}");
            }

            return Task.CompletedTask; 
        }
    }
}