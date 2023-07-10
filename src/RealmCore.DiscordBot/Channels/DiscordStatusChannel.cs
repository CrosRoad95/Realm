using Grpc.Core;
using RealmCore.Discord.Integration.Extensions;

namespace RealmCore.Discord.Integration.Channels;

public sealed class DiscordStatusChannel : ChannelBase
{
    private readonly DiscordBotOptions.StatusChannelConfiguration? _configuration;
    private readonly BotIdProvider _botIdProvider;
    private readonly ILogger _logger;
    private IUserMessage? _statusDiscordMessage;
    private readonly StatusChannel.StatusChannelClient _statusChannelClient;
    private TimestampTag? _lastReceivedUpdate = null;

    public delegate Task<string> GetStatusChannelContent();

    public DiscordStatusChannel(IOptions<DiscordBotOptions> discordConfiguration, BotIdProvider botIdProvider, ILogger<DiscordStatusChannel> logger, GrpcChannel grpcChannel)
    {
        _configuration = discordConfiguration.Value.StatusChannel;
        _botIdProvider = botIdProvider;
        _statusChannelClient = new(grpcChannel);
        _logger = logger;
    }

    public override async Task StartAsync(SocketGuild socketGuild)
    {
        if (_configuration == null)
            return;

        var channel = socketGuild.GetChannel(_configuration.ChannelId) as SocketTextChannel;
        _statusDiscordMessage = await channel.TryGetLastMessageSendByUser(_botIdProvider.Id) as IUserMessage;
        if (_statusDiscordMessage == null)
            _statusDiscordMessage = await channel.SendMessageAsync(string.Empty);

        var timer = new PeriodicTimer(TimeSpan.FromSeconds(10));
        await Update();
        while (await timer.WaitForNextTickAsync())
            await Update();
    }

    public async Task Update()
    {
        var timestamp = new TimestampTag(DateTime.UtcNow, TimestampTagStyles.ShortDateTime);
        if (_statusDiscordMessage == null)
            throw new Exception("Failed to update status channel, channel not found.");

        string newContent = "";
        try
        {
            var response = await _statusChannelClient.UpdateAsync(new(), deadline: DateTime.UtcNow.AddSeconds(2));
            newContent = response.Message;
            _lastReceivedUpdate = timestamp;
        }
        catch (RpcException ex) when (ex.StatusCode == StatusCode.DeadlineExceeded)
        {
            newContent = $"Stan serwera na dzień {timestamp}: Serwer nie odpowiada, prawdopodobnie jest wyłączony lub wystąpił inny nieznany błąd";
            _logger.LogError(ex, "Error while connecting user");
            if (_lastReceivedUpdate != null)
                newContent += $"\nOstatnio serwer był online dnia: {_lastReceivedUpdate}";
        }
        catch (Exception ex)
        {
            newContent = $"Stan serwera na dzień {timestamp}: Wystąpił nieznany błąd podczas pobierania statusu serwera.";
            _logger.LogError(ex, "Failed to update status channel.");
            if (_lastReceivedUpdate != null)
                newContent += $"\nOstatnio serwer był online dnia: {_lastReceivedUpdate}";
        }
        finally
        {
            if (!string.IsNullOrEmpty(newContent))
                await _statusDiscordMessage.ModifyAsync(x => x.Content = newContent);
        }
    }
}
