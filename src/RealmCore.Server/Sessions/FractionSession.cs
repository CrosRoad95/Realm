namespace RealmCore.Server.Sessions;

public class FractionSession : SessionBase
{
    private readonly Player _player;
    public FractionSession(string code, Player player) : base(code)
    {
        _player = player;
    }
}
