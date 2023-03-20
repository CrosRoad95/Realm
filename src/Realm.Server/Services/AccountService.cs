using Microsoft.AspNetCore.Authorization;

namespace Realm.Server.Services;

internal class AccountService : IAccountService
{
    private readonly IAuthorizationService _authorizationService;
    private readonly SignInManager<User> _signInManager;

    public AccountService(IAuthorizationService authorizationService, SignInManager<User> signInManager)
    {
        _authorizationService = authorizationService;
        _signInManager = signInManager;
    }

    public async Task<bool> AuthorizePolicy(AccountComponent accountComponent, string policy)
    {
        var result = await _authorizationService.AuthorizeAsync(accountComponent.ClaimsPrincipal, policy);
        return result.Succeeded;
    }
}
