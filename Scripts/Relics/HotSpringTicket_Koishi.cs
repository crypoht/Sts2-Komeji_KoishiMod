using BaseLib.Utils;
using KomeijiKoishi.Pools;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Nodes.CommonUi;
using MegaCrit.Sts2.Core.Rooms;

namespace KomeijiKoishi.Relics;

[Pool(typeof(KoishiSharedRelicPool))]
public sealed class HotSpringTicket_Koishi : KoishiCollaborationRelic
{
    public override async Task AfterRoomEntered(AbstractRoom room)
    {
        if (room is not RestSiteRoom || Owner.Creature.IsDead)
            return;

        Flash();
        var missingHp = Owner.Creature.MaxHp - Owner.Creature.CurrentHp;
        if (missingHp > 0)
            await CreatureCmd.Heal(Owner.Creature, missingHp, true);

        var eligible = PileType.Deck.GetPile(Owner).Cards.Where(card => card.IsUpgradable).ToList();
        if (eligible.Count > 0 && Owner.PlayerRng.Rewards.NextItem(eligible) is { } card)
            CardCmd.Upgrade(card, CardPreviewStyle.GridLayout);
    }
}
