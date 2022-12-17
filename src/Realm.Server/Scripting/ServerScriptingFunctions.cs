using Realm.Domain;
using Realm.Domain.New;

namespace Realm.Server.Scripting;

[NoDefaultScriptAccess]
public class ServerScriptingFunctions
{
    private readonly IInternalRPGServer _internalRPGServer;

    public ServerScriptingFunctions(IInternalRPGServer internalRPGServer)
    {
        _internalRPGServer = internalRPGServer;
    }

    [ScriptMember("createEntity")]
    public Entity CreateEntity(string name) => _internalRPGServer.CreateEntity(name);
}
