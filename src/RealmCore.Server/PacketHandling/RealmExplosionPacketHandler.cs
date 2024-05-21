namespace RealmCore.Server.PacketHandling;

public class RealmExplosionPacketHandler : IPacketHandler<ExplosionPacket>
{
    private readonly ISyncHandlerMiddleware<ExplosionPacket> _middleware;
    private readonly IElementCollection _elementCollection;
    private readonly IAntiCheat _antiCheat;

    public PacketId PacketId => PacketId.PACKET_ID_EXPLOSION;

    public RealmExplosionPacketHandler(ISyncHandlerMiddleware<ExplosionPacket> middleware, IElementCollection elementCollection, IAntiCheat antiCheat
    )
    {
        _middleware = middleware;
        _elementCollection = elementCollection;
        _antiCheat = antiCheat;
    }

    public void HandlePacket(IClient client, ExplosionPacket packet)
    {
        var player = (RealmPlayer)client.Player;
        packet.PlayerSource = player.Id;

        var distance = (player.Position - packet.Position).LengthSquared();
        if(distance > 500 * 500)
        {
            _antiCheat.ReportViolation(player, KnownAntiCheatViolation.ExplosionOutsideRange);
            return;
        }

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
