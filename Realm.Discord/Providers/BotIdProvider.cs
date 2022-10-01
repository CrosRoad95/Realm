namespace Realm.Discord.Providers;

internal class BotIdProvider : IBotdIdProvider
{
    public static ulong BotId { get; set; }
    public BotIdProvider()
    {

    }

    public ulong Provide() => BotId;
}
