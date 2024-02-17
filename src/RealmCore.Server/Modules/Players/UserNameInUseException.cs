namespace RealmCore.Server.Modules.Players;

public class UserNameInUseException : Exception
{
    private readonly string _userName;

    public UserNameInUseException(string userName)
    {
        _userName = userName;
    }
}
