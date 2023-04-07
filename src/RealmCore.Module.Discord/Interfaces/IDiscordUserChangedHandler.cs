namespace RealmCore.Module.Discord.Interfaces;

public interface IDiscordUserChangedHandler
{
    Task HandleUserChanged(ulong discordUserId);
}
