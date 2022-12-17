namespace Realm.Domain.Sessions;

[NoDefaultScriptAccess]
public class FractionSession : SessionBase
{
    private readonly Player _rpgPlayer;
    public FractionSession(string code, Player player) : base(code)
    {
        _rpgPlayer = player;
    }
}
