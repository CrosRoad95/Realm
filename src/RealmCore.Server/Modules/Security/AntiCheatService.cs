namespace RealmCore.Server.Modules.Security;

internal sealed class AntiCheatService : BackgroundService
{
    public AntiCheatService(MtaServer mtaServer)
    {
        mtaServer.RegisterPacketHandler<RealmExplosionPacketHandler, ExplosionPacket>();
        mtaServer.PlayerJoined += HandlePlayerJoined;
    }

    private void HandlePlayerJoined(Player player)
    {
        player.ResendModPackets();
        player.ResendPlayerACInfo();
    }

    protected override Task ExecuteAsync(CancellationToken stoppingToken)
    {
        return Task.CompletedTask;
    }
}