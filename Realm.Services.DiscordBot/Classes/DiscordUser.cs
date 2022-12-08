namespace Realm.Services.DiscordBot.Classes;

internal class DiscordUser
{
    private readonly SocketGuildUser _user;

    public ulong Id => _user.Id;
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
