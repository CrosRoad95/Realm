namespace Realm.Server.Services;

internal class DiscordUserChangedHandler : IDiscordUserChangedHandler
{
    private readonly UserManager<User> _userManager;
    private readonly AccountsInUseService _accountsInUseService;
    private readonly EventScriptingFunctions _eventFunctions;
    private readonly IdentityScriptingFunctions _identityFunctions;

    public DiscordUserChangedHandler(UserManager<User> userManager, AccountsInUseService accountsInUseService, EventScriptingFunctions eventFunctions, IdentityScriptingFunctions identityFunctions)
    {
        _userManager = userManager;
        _accountsInUseService = accountsInUseService;
        _eventFunctions = eventFunctions;
        _identityFunctions = identityFunctions;
    }

    public async Task Handle(IDiscordUser discordUser)
    {
        var users = await _userManager.GetUsersForClaimAsync(new Claim(PlayerAccount.ClaimDiscordUserIdName, discordUser.Id.ToString()));
        if (!users.Any())
            return;

        var user = users.First();
        _accountsInUseService.TryGetPlayerByAccountId(user.Id.ToString().ToUpper(), out RPGPlayer? rpgPlayer); // Fast lookup
        if(rpgPlayer != null && rpgPlayer.Account != null && rpgPlayer.Account.Discord != null)
        {
            using var discord = new DiscordUserChangedEvent(rpgPlayer.Account, rpgPlayer.Account.Discord);
            await _eventFunctions.InvokeEvent(discord);
        }
        else // Player is offline or discord account not found?
        {
            var account = await _identityFunctions.FindAccountById(user.Id.ToString().ToUpper()) ?? throw new Exception("Failed to handle user change, maybe a bug?");
            await account.UpdateClaimsPrincipal();
            account.TryInitializeDiscordUser();

            if (account.Discord == null)
                return; // TODO: Do something, log exception because maybe here a discord should not be null

            using var discord = new DiscordUserChangedEvent(account, account.Discord);
            await _eventFunctions.InvokeEvent(discord);
        }
    }
}
