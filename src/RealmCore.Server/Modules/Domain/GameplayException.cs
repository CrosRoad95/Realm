namespace RealmCore.Server.Modules.Domain;

public class GameplayException : Exception
{
    public GameplayException(string message) : base(message) { }
}
