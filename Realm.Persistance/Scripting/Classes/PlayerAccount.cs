using Microsoft.AspNetCore.Identity;
using Newtonsoft.Json.Linq;
using Realm.Persistance.Data;
using System.Security.Claims;

namespace Realm.Persistance.Scripting.Classes;

public class PlayerAccount : IDisposable
{
    private readonly User _user;
    private readonly UserManager<User> _userManager;
    private bool _disposed;

    [NoScriptAccess]
    public User User => _user;

    public string Id => _user.Id.ToString();
    public string UserName => _user.UserName;
    public PlayerAccount(User user, UserManager<User> userManager)
    {
        _user = user;
        _userManager = userManager;
    }

    public override string ToString() => _user.ToString();

    public async Task<bool> Delete()
    {
        if (_disposed)
            throw new ObjectDisposedException(GetType().ShortDisplayName());

        var result = await _userManager.DeleteAsync(_user);
        if(result.Succeeded)
        {
            Dispose();
            return true;
        }
        return false;
    }

    public async Task<bool> AddClaim(string type, string value)
    {
        if (_disposed)
            throw new ObjectDisposedException(GetType().ShortDisplayName());

        var result = await _userManager.AddClaimAsync(_user, new Claim(type, value));
        return result.Succeeded;
    }

    public async Task<bool> AddClaims(Dictionary<string, string> claims)
    {
        if (_disposed)
            throw new ObjectDisposedException(GetType().ShortDisplayName());

        var result = await _userManager.AddClaimsAsync(_user, claims.Select(x => new Claim(x.Key, x.Value)));
        return result.Succeeded;
    }

    public async Task<bool> AddRole(string role)
    {
        if (_disposed)
            throw new ObjectDisposedException(GetType().ShortDisplayName());

        var result = await _userManager.AddToRoleAsync(_user, role);
        return result.Succeeded;
    }
    
    public async Task<bool> AddRoles(IEnumerable<string> role)
    {
        if (_disposed)
            throw new ObjectDisposedException(GetType().ShortDisplayName());

        var result = await _userManager.AddToRolesAsync(_user, role);
        return result.Succeeded;
    }

    public async Task<object> GetClaims()
    {
        if (_disposed)
            throw new ObjectDisposedException(GetType().ShortDisplayName());

        return (await _userManager.GetClaimsAsync(_user)).Select(x => x.Type).ToArray().ToScriptArray();
    }
    
    public async Task<object> GetRoles()
    {
        if (_disposed)
            throw new ObjectDisposedException(GetType().ShortDisplayName());

        return (await _userManager.GetRolesAsync(_user)).ToArray().ToScriptArray();
    }

    public async Task<bool> RemoveClaim(string type, string? value = null)
    {
        if (_disposed)
            throw new ObjectDisposedException(GetType().ShortDisplayName());
        var claims = await _userManager.GetClaimsAsync(_user);
        Claim? claim;
        if (value != null)
            claim = claims.FirstOrDefault(x => x.Type == type && x.Value == value);
        else
            claim = claims.FirstOrDefault(x => x.Type == type);

        if(claim != null)
        {
            var result = await _userManager.RemoveClaimAsync(_user, claim);
            return result.Succeeded;
        }
        return false;
    }

    public async Task<bool> RemoveRole(string role)
    {
        if (_disposed)
            throw new ObjectDisposedException(GetType().ShortDisplayName());

        var result = await _userManager.RemoveFromRoleAsync(_user, role);
        return result.Succeeded;
    }

    public async Task<bool> RemoveAllClaims()
    {
        var claims = await _userManager.GetClaimsAsync(_user);
        var result = await _userManager.RemoveClaimsAsync(_user, claims);
        return result.Succeeded;
    }

    [NoScriptAccess]
    public void Dispose()
    {
        _disposed = true;
    }
}
