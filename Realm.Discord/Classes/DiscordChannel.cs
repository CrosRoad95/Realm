namespace Realm.Discord.Classes;

internal class DiscordChannel : IDiscordChannel
{
    private readonly SocketTextChannel _socketGuildChannel;

    public DiscordChannel(SocketGuildChannel socketGuildChannel)
    {
        _socketGuildChannel = (socketGuildChannel as SocketTextChannel) ?? throw new ArgumentNullException(nameof(socketGuildChannel));
    }

    public async Task<IDiscordMessage> SendMessage(string message)
    {
        return new DiscordMessage((await _socketGuildChannel.SendMessageAsync(message)));
    }

    public async Task<IDiscordMessage?> GetLastMessageSendByUser(ulong userId)
    {
        IEnumerable<IMessage> lastMessaged = _socketGuildChannel.GetCachedMessages();
        if(lastMessaged.TryGetNonEnumeratedCount(out var count))
        {
            if (count == 0)
            {
                lastMessaged = await _socketGuildChannel.GetMessagesAsync().FlattenAsync();
            }
        }
        var message = lastMessaged.FirstOrDefault(x => x.Author.Id == userId);
        if (message == null)
            return null;
        return new DiscordMessage(message);
    }
}
