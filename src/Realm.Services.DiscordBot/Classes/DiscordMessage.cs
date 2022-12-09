namespace Realm.Services.DiscordBot.Classes;

internal class DiscordMessage
{
    private readonly IMessage _message;

    public DiscordMessage(IMessage message)
    {
        _message = message;
    }

    public async Task Modify(string newContent)
    {
        await (_message as IUserMessage).ModifyAsync(m => m.Content = newContent);
    }
}
