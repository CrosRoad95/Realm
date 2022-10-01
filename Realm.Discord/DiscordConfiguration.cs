namespace Realm.Discord;

internal class DiscordConfiguration
{
    internal class StatusChannelConfiguration
    {
        public ulong ChannelId { get; set; }
    }

    public string Token { get; set; } = "";
    public ulong Guild { get; set; }

    public StatusChannelConfiguration StatusChannel { get; set; } = default!;
}
