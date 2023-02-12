namespace Realm.DiscordBot;

internal interface IChannelConfiguration
{
    public ulong ChannelId { set; }

}

internal class DiscordBotConfiguration
{
    public class StatusChannelConfiguration : IChannelConfiguration
    {
        public ulong ChannelId { get; set; }
    }

    public class ConnectServerAccountConfiguration : IChannelConfiguration
    {
        public ulong ChannelId { get; set; }
        public string InformationMessage { get; set; }
        public string SuccessMessage { get; set; }
    }

    public string Token { get; set; } = "";
    public ulong Guild { get; set; }
    public string GrpcAddress { get; set; }

    public StatusChannelConfiguration? StatusChannel { get; set; }
    public ConnectServerAccountConfiguration? ConnectServerAccountChannel { get; set; }
}
