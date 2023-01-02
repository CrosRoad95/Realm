namespace Realm.Domain.Components.Players;

public class AccountComponent : Component
{
    [Inject]
    private UserManager<User> UserManager { get; set; } = default!;

    [Inject]
    private SignInManager<User> SignInManager { get; set; } = default!;

    private readonly User _user;
    private ClaimsPrincipal? _claimsPrincipal;

    public Guid Id => _user.Id;

    public string? UserName => _user.UserName;

    public User User => _user;

    public AccountComponent(User user)
    {
        _user = user;
    }

    public override async Task Load()
    {
        if(Entity.TryGetComponent(out PlayerElementComponent playerElementComponent))
        {
            var client = playerElementComponent.Player.Client;
            _user.LastLogindDateTime = DateTime.Now;
            _user.LastIp = client.IPAddress?.ToString();
            _user.LastSerial = client.Serial;
            if(_user.RegisterSerial == null)
                _user.RegisterSerial = client.Serial;
            
            if(_user.RegisterIp == null)
                _user.RegisterIp = _user.LastIp;

            if(_user.RegisteredDateTime == null)
                _user.RegisteredDateTime = DateTime.Now;

            await UserManager.UpdateAsync(_user);
        }
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
}
