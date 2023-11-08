using RealmCore.Persistence.Data.Helpers;

namespace RealmCore.Server.Interfaces.Players;

public interface IPlayerUserService : IPlayerService
{
    internal UserData User { get; }
    ClaimsPrincipal ClaimsPrincipal { get; }
    int Id { get; }
    string Nick { get; }
    string UserName { get; }
    DateTime? LastNewsReadDateTime { get; }
    bool IsSignedIn { get; }
    TransformAndMotion? LastTransformAndMotion { get; }

    event Action<IPlayerUserService>? SignedIn;
    event Action<IPlayerUserService>? SignedOut;

    void AddAuthorizedPolicy(string policy, bool authorized);
    bool AddClaim(string type, string value);
    bool AddClaims(Dictionary<string, string> claims);
    bool AddRole(string role);
    bool AddRoles(IEnumerable<string> roles);
    IReadOnlyList<string> GetClaims();
    string? GetClaimValue(string type);
    IReadOnlyList<string> GetRoles();
    bool HasAuthorizedPolicies(string[] policies);
    bool HasAuthorizedPolicy(string policy, out bool authorized);
    bool HasClaim(string type, string? value = null);
    bool IsInRole(string role);
    void SignIn(UserData user, ClaimsPrincipal claimsPrincipal);
    void SignOut();
    bool TryRemoveClaim(string type, string? value = null);
    bool TryRemoveRole(string role);
    void UpdateLastNewsRead();
}
