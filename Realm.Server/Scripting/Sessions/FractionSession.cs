using SlipeServer.Server.Elements;

namespace Realm.Server.Scripting.Sessions;

[NoDefaultScriptAccess]
public class FractionSession : SessionBase
{
    private readonly RPGPlayer _player;
    public FractionSession(string code, RPGPlayer player) : base(code)
    {
        _player = player;
    }
}
