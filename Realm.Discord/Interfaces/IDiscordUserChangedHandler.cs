namespace Realm.Discord.Interfaces;

public interface IDiscordUserChangedHandler
{
    Task Handle(IDiscordUser discordUser);
}
