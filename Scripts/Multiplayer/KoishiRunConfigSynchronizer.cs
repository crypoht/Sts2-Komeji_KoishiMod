using System.Collections.Generic;
using MegaCrit.Sts2.Core.Logging;
using MegaCrit.Sts2.Core.Multiplayer.Game;

namespace KomeijiKoishi.Multiplayer;

public static class KoishiRunConfigSynchronizer
{
    private static readonly HashSet<INetGameService> RegisteredServices = new();

    public static void Register(INetGameService netService)
    {
        if (!RegisteredServices.Add(netService))
        {
            return;
        }

        netService.RegisterMessageHandler<KoishiRunConfigMessage>(HandleMessage);
        netService.Disconnected += _ => RegisteredServices.Remove(netService);
    }

    public static void BroadcastHostConfig(INetGameService netService)
    {
        KoishiRunConfigMessage message = KoishiModConfig.CreateRunConfigMessage();
        KoishiModConfig.ApplySyncedRunConfig(message);

        if (netService.IsConnected && netService.Type.IsMultiplayer())
        {
            netService.SendMessage(message);
            Log.Debug("[KoishiConfigSync] Broadcasted host run config.");
        }
    }

    private static void HandleMessage(KoishiRunConfigMessage message, ulong senderId)
    {
        KoishiModConfig.ApplySyncedRunConfig(message);
        Log.Debug($"[KoishiConfigSync] Accepted host run config from {senderId}.");
    }
}
