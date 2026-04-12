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
    public sealed class RoseBloom_Koishi : CustomCardModel
    {
        public RoseBloom_Koishi()
            : base(1, CardType.Attack, CardRarity.Common, TargetType.AnyEnemy, true)
        {
        }

        public MegaCrit.Sts2.Core.Localization.LocString GetSafePrompt()
        {
            return base.SelectionScreenPrompt;
        }

        public override string PortraitPath => $"res://mods/Komeiji_Koishi/images/cards/{GetType().Name}.png";

        public override IEnumerable<CardKeyword> CanonicalKeywords => new[] { KoishiKeywords.Bloom };

        protected override IEnumerable<DynamicVar> CanonicalVars => new List<DynamicVar>
        {
            new DamageVar(7m, ValueProp.Move)
        };

        protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
        {
            await DamageCmd.Attack(base.DynamicVars.Damage.BaseValue).FromCard(this).Targeting(cardPlay.Target!).WithHitFx("vfx/vfx_attack_slash").Execute(choiceContext);

            var ownerProp = typeof(CardModel).GetProperty("Owner", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            var player = (MegaCrit.Sts2.Core.Entities.Players.Player)ownerProp!.GetValue(this)!;

            if (player != null)
            {
                await BloomStancePower.EnterThisStance(choiceContext, player, this);
            }
        }

        protected override void OnUpgrade()
        {
            foreach (var kvp in base.DynamicVars)
            {
                if (kvp.Value is DamageVar dv)
                {
                    dv.UpgradeValueBy(3m); 
                    break;
                }
            }
        }
    }
}