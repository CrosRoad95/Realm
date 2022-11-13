namespace Realm.Discord.Classes;

internal class DiscordUser : IDiscordUser
{
    private readonly SocketGuildUser _user;

    public string Username => _user.Username;
    public DiscordUser(SocketGuildUser user)
    {
        _user = user;
    }

    public async Task SendTextMessage(string text)
    {
        await _user.SendMessageAsync(text);
    }
}
