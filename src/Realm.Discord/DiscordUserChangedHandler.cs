using Microsoft.AspNetCore.Identity;
using Realm.Persistance.Data;
using System.Security.Claims;
using Realm.Module.Scripting.Extensions;
using Realm.Module.Discord.Scripting.Events;

namespace Realm.Module.Discord;

internal class DiscordUserChangedHandler
{
    private readonly UserManager<User> _userManager;
    private readonly EventScriptingFunctions _eventFunctions;
    private readonly IdentityScriptingFunctions _identityFunctions;

    public DiscordUserChangedHandler(UserManager<User> userManager, EventScriptingFunctions eventFunctions, IdentityScriptingFunctions identityFunctions)
    {
        _userManager = userManager;
        _eventFunctions = eventFunctions;
        _identityFunctions = identityFunctions;
    }

    public async Task Handle(DiscordUser discordUser)
    {
        var users = await _userManager.GetUsersForClaimAsync(new Claim(PlayerAccount.ClaimDiscordUserIdName, discordUser.Id.ToString()));
        if (!users.Any())
            return;

        var user = users.First();
        var account = await _identityFunctions.FindAccountById(user.Id.ToString().ToUpper()) ?? throw new Exception("Failed to handle user change, maybe a bug?");
        await account.UpdateClaimsPrincipal();
        account.TryInitializeDiscordUser();

        if (account.Discord == null)
            return; // TODO: Do something, log exception because maybe here a discord should not be null

        using var discord = new DiscordUserChangedEvent(account, account.Discord);
        await _eventFunctions.InvokeEvent(discord);
    }
}
