using Microsoft.EntityFrameworkCore;
using Realm.Module.Scripting.Extensions;
using Realm.Persistance;
using System.Security.Claims;

namespace Realm.Domain.Components.Players;

[NoDefaultScriptAccess]
public class AccountComponent : Component
{
    private readonly User _user;
    private ClaimsPrincipal _claimsPrincipal;


    [ScriptMember("id")]
    public string Id => _user.Id.ToString().ToUpper();

    [ScriptMember("userName")]
    public string? UserName => _user.UserName;

    [ScriptMember("registerDateTime")]
    public DateTime? RegisterDateTime => _user.RegisteredDateTime;

    public AccountComponent(User user, ClaimsPrincipal claimsPrincipal)
    {
        _user = user;
        _claimsPrincipal = claimsPrincipal;
    }

    public async Task UpdateClaimsPrincipal()
    {
        var signInManager = Entity.GetRequiredService<SignInManager<User>>();
        _claimsPrincipal = await signInManager.CreateUserPrincipalAsync(_user);
    }

    [ScriptMember("isInRole")]
    public bool IsInRole(string role)
    {
        return _claimsPrincipal!.IsInRole(role);
    }

    [ScriptMember("hasClaim")]
    public bool HasClaim(string type)
    {
        return _claimsPrincipal!.HasClaim(x => x.Type == type);
    }

    [ScriptMember("getClaimValue")]
    public string? GetClaimValue(string type)
    {
        return _claimsPrincipal!.Claims.First(x => x.Type == type).Value;
    }

    [ScriptMember("addClaim")]
    public async Task<bool> AddClaim(string type, string value)
    {
        var userManager = Entity.GetRequiredService<UserManager<User>>();

        var result = await userManager.AddClaimAsync(_user, new Claim(type, value));
        if (result.Succeeded)
            await UpdateClaimsPrincipal();
        return result.Succeeded;
    }

    [ScriptMember("addClaims")]
    public async Task<bool> AddClaims(Dictionary<string, string> claims)
    {
        var userManager = Entity.GetRequiredService<UserManager<User>>();

        var result = await userManager.AddClaimsAsync(_user, claims.Select(x => new Claim(x.Key, x.Value)));
        if (result.Succeeded)
            await UpdateClaimsPrincipal();
        return result.Succeeded;
    }

    [ScriptMember("addRole")]
    public async Task<bool> AddRole(string role)
    {
        var userManager = Entity.GetRequiredService<UserManager<User>>();

        var result = await userManager.AddToRoleAsync(_user, role);
        if (result.Succeeded)
            await UpdateClaimsPrincipal();
        return result.Succeeded;
    }

    [ScriptMember("addRoles")]
    public async Task<bool> AddRoles(IEnumerable<string> role)
    {
        var userManager = Entity.GetRequiredService<UserManager<User>>();

        var result = await userManager.AddToRolesAsync(_user, role);
        if (result.Succeeded)
            await UpdateClaimsPrincipal();
        return result.Succeeded;
    }

    [ScriptMember("getClaims")]
    public async Task<object> GetClaims()
    {
        var userManager = Entity.GetRequiredService<UserManager<User>>();

        return (await userManager.GetClaimsAsync(_user)).Select(x => x.Type).ToArray().ToScriptArray();
    }

    [ScriptMember("getRoles")]
    public async Task<object> GetRoles()
    {
        var userManager = Entity.GetRequiredService<UserManager<User>>();

        return (await userManager.GetRolesAsync(_user)).ToArray().ToScriptArray();
    }

    [ScriptMember("removeClaim")]
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

    [ScriptMember("removeRole")]
    public async Task<bool> RemoveRole(string role)
    {
        var userManager = Entity.GetRequiredService<UserManager<User>>();

        var result = await userManager.RemoveFromRoleAsync(_user, role);
        if (result.Succeeded)
            await UpdateClaimsPrincipal();
        return result.Succeeded;
    }

    [ScriptMember("removeAllClaims")]
    public async Task<bool> RemoveAllClaims()
    {
        var userManager = Entity.GetRequiredService<UserManager<User>>();

        var claims = await userManager.GetClaimsAsync(_user);
        var result = await userManager.RemoveClaimsAsync(_user, claims);
        if (result.Succeeded)
            await UpdateClaimsPrincipal();
        return result.Succeeded;
    }

    [ScriptMember("hasData")]
    public async Task<bool> HasData(string key)
    {
        var db = Entity.GetRequiredService<IDb>();

        var playerData = await db.UserData
            .AsNoTrackingWithIdentityResolution()
            .FirstOrDefaultAsync(x => x.UserId.ToString() == Id && x.Key == key);
        return playerData != null;
    }

    [ScriptMember("getData")]
    public async Task<string?> GetData(string key)
    {
        var db = Entity.GetRequiredService<IDb>();

        var playerData = await db.UserData
            .AsNoTrackingWithIdentityResolution()
            .FirstOrDefaultAsync(x => x.UserId.ToString() == Id && x.Key == key);
        if (playerData == null)
            return null;
        return playerData.Value;
    }

    [ScriptMember("removeData")]
    public async Task<bool> RemoveData(string key)
    {
        var db = Entity.GetRequiredService<IDb>();

        var playerData = await db.UserData.FirstOrDefaultAsync(x => x.UserId.ToString() == Id && x.Key == key);
        if (playerData == null)
            return false;

        db.UserData.Remove(playerData);
        var savedEntities = await db.SaveChangesAsync();
        return savedEntities == 1;
    }

    [ScriptMember("setData")]
    public async Task<bool> SetData(string key, string value)
    {
        var db = Entity.GetRequiredService<IDb>();

        int savedEntities;

        var playerData = await db.UserData.FirstOrDefaultAsync(x => x.UserId.ToString() == Id && x.Key == key);
        if (playerData == null)
        {
            playerData = new UserData
            {
                Key = key,
                UserId = Guid.Parse(Id),
                Value = value
            };
            db.UserData.Add(playerData);
            savedEntities = await db.SaveChangesAsync();
            return savedEntities == 1;
        }
        if (playerData.Value == value)
            return true;

        playerData.Value = value;
        db.UserData.Update(playerData);
        savedEntities = await db.SaveChangesAsync();
        return savedEntities == 1;
    }
}
