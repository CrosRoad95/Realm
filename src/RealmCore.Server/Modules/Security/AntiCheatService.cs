namespace RealmCore.Server.Modules.Security;

internal sealed class AntiCheatService : BackgroundService
{
    public AntiCheatService(MtaServer mtaServer)
    {
        mtaServer.RegisterPacketHandler<RealmExplosionPacketHandler, ExplosionPacket>();
    }

    protected override Task ExecuteAsync(CancellationToken stoppingToken)
    {
        return Task.CompletedTask;
    }
}