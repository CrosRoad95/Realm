using RealmCore.Persistence.Data.Helpers;
using RealmCore.Server.DomainObjects;

namespace RealmCore.Server.Interfaces.Players;

public interface IPlayerUserService
{
    internal UserData User { get; }
    ClaimsPrincipal ClaimsPrincipal { get; }
    int Id { get; }
    string Nick { get; }
    string UserName { get; }
    IReadOnlyList<int> Upgrades { get; }
    ICollection<int> Settings { get; }
    DateTime? LastNewsReadDateTime { get; }
    Bans Bans { get; }
    bool IsSignedIn { get; }
    TransformAndMotion? LastTransformAndMotion { get; }

    event Action<IPlayerUserService, int>? UpgradeAdded;
    event Action<IPlayerUserService, int>? UpgradeRemoved;
    event Action<IPlayerUserService, int, string>? SettingChanged;
    event Action<IPlayerUserService, int, string>? SettingRemoved;

    void AddAuthorizedPolicy(string policy, bool authorized);
    bool AddClaim(string type, string value);
    bool AddClaims(Dictionary<string, string> claims);
    bool AddRole(string role);
    bool AddRoles(IEnumerable<string> roles);
    IReadOnlyList<string> GetClaims();
    string? GetClaimValue(string type);
    IReadOnlyList<string> GetRoles();
    string? GetSetting(int settingId);
    bool HasAuthorizedPolicies(string[] policies);
    bool HasAuthorizedPolicy(string policy, out bool authorized);
    bool HasClaim(string type, string? value = null);
    bool HasUpgrade(int upgradeId);
    bool IsInRole(string role);
    bool RemoveSetting(int settingId);
    void SetSetting(int settingId, string value);
    void SignIn(UserData user, ClaimsPrincipal claimsPrincipal, Bans bans);
    void SignOut();
    bool TryAddUpgrade(int upgradeId);
    bool TryGetSetting(int settingId, out string? value);
    bool TryRemoveClaim(string type, string? value = null);
    bool TryRemoveRole(string role);
    bool TryRemoveUpgrade(int upgradeId);
    void UpdateLastNewsRead();
}
