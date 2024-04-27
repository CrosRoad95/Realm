namespace RealmCore.Discord.Integration;

internal interface IChannelConfiguration
{
    public ulong ChannelId { get; }
}

public sealed class DiscordBotOptions
{
    public sealed class StatusChannelConfiguration : IChannelConfiguration
    {
        public required ulong ChannelId { get; init; }
    }

    public sealed class ConnectServerUserOptions : IChannelConfiguration
    {
        public required ulong ChannelId { get; init; }
        public required string InformationMessage { get; init; }
        public required string SuccessMessage { get; init; }
    }

    public required string Token { get; init; } = "";
    public required ulong Guild { get; init; }
    public required string TextBasedCommandPrefix { get; init; } = "&";

    public StatusChannelConfiguration? StatusChannel { get; init; }
    public ConnectServerUserOptions? ConnectServerUserChannel { get; init; }
}
