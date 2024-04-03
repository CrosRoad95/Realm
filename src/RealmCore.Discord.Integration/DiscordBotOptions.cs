namespace RealmCore.Discord.Integration;

internal interface IChannelConfiguration
{
    public ulong ChannelId { set; }

}

public sealed class DiscordBotOptions
{
    public class StatusChannelConfiguration : IChannelConfiguration
    {
        public ulong ChannelId { get; set; }
    }

    public class ConnectServerUserOptions : IChannelConfiguration
    {
        public ulong ChannelId { get; set; }
        public string InformationMessage { get; set; }
        public string SuccessMessage { get; set; }
    }

    public string Token { get; set; } = "";
    public ulong Guild { get; set; }
    public string GrpcAddress { get; set; }
    public string TextBasedCommandPrefix { get; set; } = "&";


    public StatusChannelConfiguration? StatusChannel { get; set; }
    public ConnectServerUserOptions? ConnectServerUserChannel { get; set; }
}
