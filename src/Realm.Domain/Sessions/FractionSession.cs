namespace Realm.Domain.Sessions;

[NoDefaultScriptAccess]
public class FractionSession : SessionBase
{
    private readonly RPGPlayer _rpgPlayer;
    public FractionSession(string code, RPGPlayer rpgPlayer) : base(code)
    {
        _rpgPlayer = rpgPlayer;
    }
}
