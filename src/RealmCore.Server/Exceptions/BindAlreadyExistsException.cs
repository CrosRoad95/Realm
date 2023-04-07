namespace RealmCore.Server.Exceptions;

public class BindAlreadyExistsException : Exception
{
    public BindAlreadyExistsException(string key) : base($"Bind with key '{key}' already exists.")
    {
    }
}
