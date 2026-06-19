using System.Collections.Generic;
using System.Threading.Tasks;
using BaseLib.Abstracts;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using KomeijiKoishi.Pools;
using KomeijiKoishi.Powers;
using KomeijiKoishi.Enums;
using MegaCrit.Sts2.Core.HoverTips;
using BaseLib.Utils;


namespace KomeijiKoishi.Cards
{
    [Pool(typeof(KoishiCardPool))]
    public sealed class InstinctiveForm_Koishi : CustomCardModel
    {
        public InstinctiveForm_Koishi() 
            : base(1, CardType.Power, CardRarity.Rare, TargetType.Self, true)
        {
        }

        protected override HashSet<CardTag> CanonicalTags => new HashSet<CardTag> { KoishiTags.Subconscious };

        public override string PortraitPath => $"res://mods/Komeiji_Koishi/images/cards/{GetType().Name}.png";

        protected override IEnumerable<DynamicVar> CanonicalVars => new List<DynamicVar> 
        { 
            new PowerVar<InstinctiveFormPower>(1m) 
        };

        

        protected override IEnumerable<IHoverTip> ExtraHoverTips => new[] 
        { 
            HoverTipFactory.FromKeyword(KoishiKeywords.Unconscious) 
        };

        protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
        {
            var player = base.Owner as Player;
            if (player == null) return;

  
            await CreatureCmd.TriggerAnim(player.Creature, "Buff", player.Character!.CastAnimDelay);

            await PowerCmd.Apply<InstinctiveFormPower>(
                choiceContext,
                player.Creature, 
                base.DynamicVars["InstinctiveFormPower"].BaseValue, 
                player.Creature, 
                this, 
                false
            );
        }

        protected override void OnUpgrade()
        {
            base.AddKeyword(CardKeyword.Innate);
        }
    }
}