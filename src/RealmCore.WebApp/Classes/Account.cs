namespace RealmCore.WebApp.Classes;

public class Account
{
    public string Id { get; }
    public string UserName { get; }
    public DateTime RegisterDateTime { get; }
    public string[] Roles { get; }
    public Claim[] Claims { get; }

    public Account(string id, string userName, DateTime registerDateTime, string[] roles, Claim[] claims)
    {
        Id = id;
        UserName = userName;
        RegisterDateTime = registerDateTime;
        Roles = roles;
        Claims = claims;
    }

}
