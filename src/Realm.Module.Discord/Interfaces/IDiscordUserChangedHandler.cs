namespace Realm.Module.Discord.Interfaces;

public interface IDiscordUserChangedHandler
{
    Task HandleUserChanged(ulong discordUserId);
}
