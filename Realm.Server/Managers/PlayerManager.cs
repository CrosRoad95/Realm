using SlipeServer.Server.ElementCollections;

namespace Realm.Server.Managers;

internal class PlayerManager : IPlayerManager
{
    private readonly IRPGServer _mtaServer;
    private readonly IElementCollection _elementCollection;

    public event Action<IRPGPlayer>? PlayerJoined;
    public PlayerManager(IRPGServer mtaServer, IElementCollection elementCollection)
    {
        mtaServer.PlayerJoined += MtaServer_PlayerJoined;
        _mtaServer = mtaServer;
        _elementCollection = elementCollection;
    }

    private void MtaServer_PlayerJoined(IRPGPlayer rpgPlayer)
    {
        PlayerJoined?.Invoke(rpgPlayer);
    }

    public IRPGPlayer[] GetAll() => _elementCollection.GetByType<RPGPlayer>().ToArray();

}
