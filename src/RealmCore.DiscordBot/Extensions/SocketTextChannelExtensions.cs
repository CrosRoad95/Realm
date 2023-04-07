namespace RealmCore.DiscordBot.Extensions;

internal static class SocketTextChannelExtensions
{
    public static async Task<IMessage?> TryGetLastMessageSendByUser(this SocketTextChannel socketTextChannel, ulong userId)
    {
        IEnumerable<IMessage> lastMessaged = socketTextChannel.GetCachedMessages();
        if (lastMessaged.TryGetNonEnumeratedCount(out var count))
        {
            if (count == 0)
            {
                lastMessaged = await socketTextChannel.GetMessagesAsync().FlattenAsync();
            }
        }
        return lastMessaged.FirstOrDefault(x => x.Author.Id == userId);
    }
}
