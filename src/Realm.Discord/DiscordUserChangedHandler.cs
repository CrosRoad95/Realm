using Microsoft.AspNetCore.Identity;
using Realm.Persistance.Data;
using Realm.Domain.Persistance;

namespace Realm.Module.Discord;

internal class DiscordUserChangedHandler
{
    private readonly UserManager<User> _userManager;

    public DiscordUserChangedHandler(UserManager<User> userManager)
    {
        _userManager = userManager;
    }

    public async Task Handle(DiscordUser discordUser)
    {

    }
}
