using Realm.Persistance.Scripting.Classes;

namespace Realm.WebApp.Classes;

public class Account
{
    public PlayerAccount PlayerAccount { get; }
    public string[] Roles { get; }
    public Claim[] Claims { get; }

    private Account(PlayerAccount playerAccount, string[] roles, Claim[] claims)
    {
        PlayerAccount = playerAccount;
        Roles = roles;
        Claims = claims;
    }

    public static async Task<Account> CreateAsync(PlayerAccount playerAccount)
    {
        return new Account(playerAccount, await playerAccount.InternalGetRoles(), await playerAccount.InternalGetClaims());
    }
}
