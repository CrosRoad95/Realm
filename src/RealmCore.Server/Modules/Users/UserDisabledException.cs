namespace RealmCore.Server.Modules.Users;

public class UserDisabledException : Exception
{
    public int UserId { get; }

    public UserDisabledException(int userId)
    {
        UserId = userId;
    }
}
