namespace RealmCore.Server.Modules.Users;

public class UsersResults
{
    public record struct Registered(int id);
    public record struct FailedToRegister();
    public record struct LoggedIn(int id);
    public record struct LoggedOut(int id);
    public record struct QuickLoginDisabled();
    public record struct UserDisabled(int id);
    public record struct PlayerAlreadyLoggedIn();
    public record struct PlayerNotLoggedIn();
    public record struct UserAlreadyInUse();

}
