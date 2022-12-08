using Realm.Domain.Elements;

namespace Realm.Domain.Sessions;

[NoDefaultScriptAccess]
public class FractionSession : SessionBase
{
    private readonly RPGPlayer _player;
    public FractionSession(string code, RPGPlayer player) : base(code)
    {
        _player = player;
    }
}
