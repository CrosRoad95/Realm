namespace RealmCore.Server.Exceptions;

public class UserDisabledException : Exception
{
    public int UserId { get; }

    public UserDisabledException(int userId)
    {
        UserId = userId;
    }
}
