using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models.Powers;
using System;
using BaseLib.Abstracts; 
using KomeijiKoishi.Pools;
using System.Linq;
using MegaCrit.Sts2.Core.Entities.Powers; 
using MegaCrit.Sts2.Core.Models; 
using MegaCrit.Sts2.Core.Models.Cards; 


namespace KomeijiKoishi.Powers
{
    public sealed class KoishiKokoroPower : CustomPowerModel, IStanceListenerPower
    {
        public override PowerType Type => PowerType.Buff;
        public override PowerStackType StackType => PowerStackType.Counter;

        public override string? CustomPackedIconPath => "res://mods/Komeiji_Koishi/images/powers/KoishiKokoroPower.png";
        public override string? CustomBigIconPath => "res://mods/Komeiji_Koishi/images/powers/KoishiKokoroPower.png";
        
        protected override object InitInternalData() => new Data();

        public override Task BeforeApplied(Creature target, decimal amount, Creature? applier, CardModel? cardSource)
        {
            if (cardSource != null)
            {
                var data = base.GetInternalData<Data>();
                foreach (var v in cardSource.DynamicVars)
                {
                    if (v.Key == "JoyMaskPower") data.FreeCount = v.Value.BaseValue;
                    if (v.Key == "StrengthPower") data.BuffAmount = v.Value.BaseValue;
                    if (v.Key == "DexterityPower") data.BuffAmount = v.Value.BaseValue;
                    if (v.Key == "Cards") data.DrawAmount = v.Value.BaseValue;
                }
            }
            return Task.CompletedTask;
        }

        public async Task OnStanceChanged(bool isClosedStance, bool isBloomStance, PlayerChoiceContext context, CardModel? sourceCard)
        {
            try 
            {
                await TriggerEffects(context, sourceCard);
            }
            catch (Exception ex)
            {
                MegaCrit.Sts2.Core.Logging.Log.Error($"[KoishiKokoro] 触发效果报错: {ex}");
            }
        }

        public async Task TriggerEffects(PlayerChoiceContext context, CardModel? sourceCard)
        {
            var data = base.GetInternalData<Data>();
            var player = base.Owner.Player;
            if (player == null) return;
            this.Flash();

            for (int i = 0; i < base.Amount; i++)
            {
                var list = new List<int> { 0, 1, 2, 3 };
                int effect = player.RunState.Rng.Shuffle.NextItem(list); 

                switch (effect)
                {
                    case 0: 
                        await PowerCmd.Apply<JoyMaskPower>(base.Owner, data.FreeCount, base.Owner, sourceCard, false);
                        break;
                    case 1: 
                        await PowerCmd.Apply<StrengthPower>(base.Owner, data.BuffAmount, base.Owner, sourceCard, false);
                        break;
                    case 2: 
                        await PowerCmd.Apply<DexterityPower>(base.Owner, data.BuffAmount, base.Owner, sourceCard, false);
                        break;
                    case 3: 
                        await CardPileCmd.Draw(context, data.DrawAmount, player, false);
                        break;
                }
            }
        }

        private class Data
        {
            public decimal FreeCount = 1m;
            public decimal BuffAmount = 2m;
            public decimal DrawAmount = 1m;
        }
    }
}