namespace RealmCore.Server.Modules.Players;

public class BindDoesNotExistsException : Exception
{
    public BindDoesNotExistsException(string key) : base($"Bind with key '{key}' doesn't exists.")
    {
    }
}
