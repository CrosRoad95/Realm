namespace Realm.Discord.Classes;

internal class DiscordUser : IDiscordUser
{
    private readonly IUser _user;

    public string Username => _user.Username;
    public DiscordUser(IUser user)
    {
        _user = user;
    }

    public async Task SendTextMessage(string text)
    {
        await _user.SendMessageAsync(text);
    }
}
