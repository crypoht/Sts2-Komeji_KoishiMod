using System.Collections.Generic;
using System.Threading.Tasks;
using System.Reflection;
using BaseLib.Utils;
using BaseLib.Abstracts;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Modding;
using MegaCrit.Sts2.Core.ValueProps;
using KomeijiKoishi.Pools;
using KomeijiKoishi.Powers;
using KomeijiKoishi.Enums;

namespace KomeijiKoishi.Cards
{
    [Pool(typeof(KoishiCardPool))]
    public sealed class HeartClosed_Koishi : CustomCardModel
    {
        public HeartClosed_Koishi()
            : base(1, CardType.Skill, CardRarity.Common, TargetType.Self, true)
        {
        }

        public override string PortraitPath => $"res://mods/Komeiji_Koishi/images/cards/{GetType().Name}.png";

        public override IEnumerable<CardKeyword> CanonicalKeywords => new[] { KoishiKeywords.Closed };

        protected override IEnumerable<DynamicVar> CanonicalVars => new List<DynamicVar>
        {
            new BlockVar(6m, ValueProp.Move) 
        };

       protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
        {
            foreach (var kvp in base.DynamicVars)
            {
                if (kvp.Value is BlockVar bv) { await CreatureCmd.GainBlock(base.Owner.Creature, bv, cardPlay, false); break; }
            }

            var ownerProp = typeof(CardModel).GetProperty("Owner", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            var player = (MegaCrit.Sts2.Core.Entities.Players.Player)ownerProp!.GetValue(this)!;

            if (player != null)
            {
                await ClosedStancePower.EnterThisStance(choiceContext, player, this);
            }
        }

        protected override void OnUpgrade()
        {
            foreach (var kvp in base.DynamicVars)
            {
                if (kvp.Value is BlockVar bv)
                {
                    bv.UpgradeValueBy(4m); 
                    break;
                }
            }
        }
    }
}