using Realm.Persistance;
using System.Collections.Generic;

namespace Realm.Domain.Components.Players;

public class AccountComponent : Component
{
    [Inject]
    private UserManager<User> UserManager { get; set; } = default!;

    [Inject]
    private SignInManager<User> SignInManager { get; set; } = default!;

    private readonly User _user;
    private ClaimsPrincipal? _claimsPrincipal;

    internal User User => _user;
    public Guid Id => _user.Id;
    public string? UserName => _user.UserName;
    public IEnumerable<string> JobUpgrades => _user.JobUpgrades.Select(x => x.Name);

    public event Action<Entity, short, string>? JobUpgradeAdded;
    public AccountComponent(User user)
    {
        _user = user;
    }

    public override async Task Load()
    {
        await UpdateClaimsPrincipal();
    }

    public async Task UpdateClaimsPrincipal()
    {
        _claimsPrincipal = await SignInManager.CreateUserPrincipalAsync(_user);
    }

    public bool IsInRole(string role)
    {
        return _claimsPrincipal!.IsInRole(role);
    }

    public bool HasClaim(string type)
    {
        return _claimsPrincipal!.HasClaim(x => x.Type == type);
    }

    public string? GetClaimValue(string type)
    {
        return _claimsPrincipal!.Claims.First(x => x.Type == type).Value;
    }

    public async Task<bool> AddClaim(string type, string value)
    {
        var result = await UserManager.AddClaimAsync(_user, new Claim(type, value));
        if (result.Succeeded)
            await UpdateClaimsPrincipal();
        return result.Succeeded;
    }

    public async Task<bool> AddClaims(Dictionary<string, string> claims)
    {
        var result = await UserManager.AddClaimsAsync(_user, claims.Select(x => new Claim(x.Key, x.Value)));
        if (result.Succeeded)
            await UpdateClaimsPrincipal();
        return result.Succeeded;
    }

    public async Task<bool> AddRole(string role)
    {
        var result = await UserManager.AddToRoleAsync(_user, role);
        if (result.Succeeded)
            await UpdateClaimsPrincipal();
        return result.Succeeded;
    }

    public async Task<bool> AddRoles(IEnumerable<string> role)
    {
        var result = await UserManager.AddToRolesAsync(_user, role);
        if (result.Succeeded)
            await UpdateClaimsPrincipal();
        return result.Succeeded;
    }

    public async Task<IEnumerable<string>> GetClaims()
    {
        return (await UserManager.GetClaimsAsync(_user)).Select(x => x.Type).ToList();
    }

    public async Task<IEnumerable<string>> GetRoles()
    {
        return (await UserManager.GetRolesAsync(_user)).ToList();
    }

    public async Task<bool> RemoveClaim(string type, string? value = null)
    {
        var claims = await UserManager.GetClaimsAsync(_user);
        Claim? claim;
        if (value != null)
            claim = claims.FirstOrDefault(x => x.Type == type && x.Value == value);
        else
            claim = claims.FirstOrDefault(x => x.Type == type);

        if (claim != null)
        {
            var result = await UserManager.RemoveClaimAsync(_user, claim);
            if (result.Succeeded)
                await UpdateClaimsPrincipal();
            return result.Succeeded;
        }
        return false;
    }

    public async Task<bool> RemoveRole(string role)
    {
        var result = await UserManager.RemoveFromRoleAsync(_user, role);
        if (result.Succeeded)
            await UpdateClaimsPrincipal();
        return result.Succeeded;
    }

    public async Task<bool> RemoveAllClaims()
    {
        var claims = await UserManager.GetClaimsAsync(_user);
        var result = await UserManager.RemoveClaimsAsync(_user, claims);
        if (result.Succeeded)
            await UpdateClaimsPrincipal();
        return result.Succeeded;
    }

    public bool HasJobUpgrade(short jobId, string upgradeName) => _user.JobUpgrades.Any(x => x.JobId == jobId && x.Name == upgradeName);

    public void AddJobUpgrade(short jobId, string upgradeName)
    {
        if (HasJobUpgrade(jobId, upgradeName))
            throw new UpgradeAlreadyExistsException(jobId, upgradeName);

        _user.JobUpgrades.Add(new JobUpgrade
        {
            UserId = _user.Id,
            User = _user,
            JobId = jobId,
            Name = upgradeName,
        });

        JobUpgradeAdded?.Invoke(Entity, jobId, upgradeName);
    }
}
