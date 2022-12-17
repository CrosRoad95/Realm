using Microsoft.AspNetCore.Identity;
using Realm.Persistance.Data;
using System.Security.Claims;
using Realm.Module.Scripting.Extensions;
using Realm.Module.Discord.Scripting.Events;
using Realm.Domain.New;

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
        using var discord = new DiscordUserChangedEvent(new DiscordComponent());
        await _eventFunctions.InvokeEvent(discord);
    }
}
