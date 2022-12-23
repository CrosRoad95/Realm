using Microsoft.EntityFrameworkCore;
using Realm.Persistance;
using System.Security.Claims;

namespace Realm.Domain.Components.Players;

public class AccountComponent : Component
{
    private readonly User _user;
    private ClaimsPrincipal? _claimsPrincipal;

    public string Id => _user.Id.ToString().ToUpper();

    public string? UserName => _user.UserName;

    public DateTime? RegisterDateTime => _user.RegisteredDateTime;

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
        var signInManager = Entity.GetRequiredService<SignInManager<User>>();
        _claimsPrincipal = await signInManager.CreateUserPrincipalAsync(_user);
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
        var userManager = Entity.GetRequiredService<UserManager<User>>();

        var result = await userManager.AddClaimAsync(_user, new Claim(type, value));
        if (result.Succeeded)
            await UpdateClaimsPrincipal();
        return result.Succeeded;
    }

    public async Task<bool> AddClaims(Dictionary<string, string> claims)
    {
        var userManager = Entity.GetRequiredService<UserManager<User>>();

        var result = await userManager.AddClaimsAsync(_user, claims.Select(x => new Claim(x.Key, x.Value)));
        if (result.Succeeded)
            await UpdateClaimsPrincipal();
        return result.Succeeded;
    }

    public async Task<bool> AddRole(string role)
    {
        var userManager = Entity.GetRequiredService<UserManager<User>>();

        var result = await userManager.AddToRoleAsync(_user, role);
        if (result.Succeeded)
            await UpdateClaimsPrincipal();
        return result.Succeeded;
    }

    public async Task<bool> AddRoles(IEnumerable<string> role)
    {
        var userManager = Entity.GetRequiredService<UserManager<User>>();

        var result = await userManager.AddToRolesAsync(_user, role);
        if (result.Succeeded)
            await UpdateClaimsPrincipal();
        return result.Succeeded;
    }

    public async Task<IEnumerable<string>> GetClaims()
    {
        var userManager = Entity.GetRequiredService<UserManager<User>>();

        return (await userManager.GetClaimsAsync(_user)).Select(x => x.Type).ToList();
    }

    public async Task<IEnumerable<string>> GetRoles()
    {
        var userManager = Entity.GetRequiredService<UserManager<User>>();

        return (await userManager.GetRolesAsync(_user)).ToList();
    }

    public async Task<bool> RemoveClaim(string type, string? value = null)
    {
        var userManager = Entity.GetRequiredService<UserManager<User>>();

        var claims = await userManager.GetClaimsAsync(_user);
        Claim? claim;
        if (value != null)
            claim = claims.FirstOrDefault(x => x.Type == type && x.Value == value);
        else
            claim = claims.FirstOrDefault(x => x.Type == type);

        if (claim != null)
        {
            var result = await userManager.RemoveClaimAsync(_user, claim);
            if (result.Succeeded)
                await UpdateClaimsPrincipal();
            return result.Succeeded;
        }
        return false;
    }

    public async Task<bool> RemoveRole(string role)
    {
        var userManager = Entity.GetRequiredService<UserManager<User>>();

        var result = await userManager.RemoveFromRoleAsync(_user, role);
        if (result.Succeeded)
            await UpdateClaimsPrincipal();
        return result.Succeeded;
    }

    public async Task<bool> RemoveAllClaims()
    {
        var userManager = Entity.GetRequiredService<UserManager<User>>();

        var claims = await userManager.GetClaimsAsync(_user);
        var result = await userManager.RemoveClaimsAsync(_user, claims);
        if (result.Succeeded)
            await UpdateClaimsPrincipal();
        return result.Succeeded;
    }
}
