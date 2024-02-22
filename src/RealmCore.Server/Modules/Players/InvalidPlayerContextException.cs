namespace RealmCore.Server.Modules.Players;

public class InvalidPlayerContextException : Exception
{
    public InvalidPlayerContextException() : base("PlayerContext has no player set") { }
}
