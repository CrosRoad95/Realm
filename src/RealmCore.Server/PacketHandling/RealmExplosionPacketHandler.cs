namespace RealmCore.Server.PacketHandling;

public class RealmExplosionPacketHandler : IPacketHandler<ExplosionPacket>
{
    private readonly ISyncHandlerMiddleware<ExplosionPacket> _middleware;
    private readonly IElementCollection _elementCollection;

    public PacketId PacketId => PacketId.PACKET_ID_EXPLOSION;

    public RealmExplosionPacketHandler(
        ISyncHandlerMiddleware<ExplosionPacket> middleware,
        IElementCollection elementCollection
    )
    {
        _middleware = middleware;
        _elementCollection = elementCollection;
    }

    public void HandlePacket(IClient client, ExplosionPacket packet)
    {
        var player = (RealmPlayer)client.Player;
        packet.PlayerSource = player.Id;

        if (packet.OriginId != null)
        {
            var explosionorigin = _elementCollection.Get(packet.OriginId.Value);
            if (explosionorigin != null)
            {
                if (explosionorigin is Vehicle vehicle)
                {
                    vehicle.BlowUp();
                }
            }
        }

        var nearbyPlayers = _middleware.GetPlayersToSyncTo(client.Player, packet);

        packet.SendTo(nearbyPlayers);
    }
}
