using Grpc.Core;
using Grpc.Net.Client;

namespace Realm.Services.DiscordBot.Channels;

internal class DiscordStatusChannel
{
    private readonly DiscordConfiguration.StatusChannelConfiguration? _configuration;
    private readonly BotIdProvider _botdIdProvider;
    private ILogger _logger;
    private DiscordMessage? _statusDiscordMessage;
    Discord.StatusChannel.StatusChannelClient _statusChannelClient;
    private TimestampTag? _lastReceivedUpdate = null;

    public delegate Task<string> GetStatusChannelContent();

    public DiscordStatusChannel(DiscordConfiguration discordConfiguration, BotIdProvider botdIdProvider, ILogger logger, GrpcChannel grpcChannel)
    {
        _configuration = discordConfiguration.StatusChannel;
        _botdIdProvider = botdIdProvider;
        _statusChannelClient = new(grpcChannel);
        _logger = logger.ForContext<DiscordStatusChannel>();
    }

    public async Task StartAsync(DiscordGuild discordGuild)
    {
        if (_configuration == null)
            return;

        var channel = discordGuild.GetChannelById(_configuration.ChannelId);
        _statusDiscordMessage = await channel.GetLastMessageSendByUser(_botdIdProvider.Id);
        if (_statusDiscordMessage == null)
            _statusDiscordMessage = await channel.SendMessage(string.Empty);

        await Update();
        while(true) // TODO: user periodic timer
        {
            await Task.Delay(TimeSpan.FromSeconds(10));
            await Update();
        }
    }
    
    public async Task Update()
    {
        var timestamp = new TimestampTag
        {
            Time = DateTime.UtcNow,
            Style = TimestampTagStyles.ShortDateTime
        };
        if (_statusDiscordMessage == null)
            throw new Exception("Failed to update status channel, channel not found.");

        string newContent = "";
        try
        {
            if(_statusDiscordMessage != null)
            {
                var cancellationToken = new CancellationTokenSource(TimeSpan.FromSeconds(5));

                var response = await _statusChannelClient.UpdateAsync(new(), null, null, cancellationToken.Token);
                newContent = response.Message;
                _lastReceivedUpdate = timestamp;
            }
        }
        catch(RpcException ex)
        {
            newContent = $"Stan serwera na dzień {timestamp}: Serwer nie odpowiada, prawdopodobnie jest wyłączony lub wystąpił inny nieznany błąd";
            if (_lastReceivedUpdate != null)
                newContent = newContent + $"\nOstatnio serwer był online dnia: {_lastReceivedUpdate}";
        }
        catch(Exception ex)
        {
            newContent = $"Stan serwera na dzień {timestamp}: Wystąpił nieznany błąd podczas pobierania statusu serwera.";
            _logger.Error(ex, "Failed to update status channel.");
            if (_lastReceivedUpdate != null)
                newContent = newContent + $"\nOstatnio serwer był online dnia: {_lastReceivedUpdate}";
        }
        finally
        {
            if (!string.IsNullOrEmpty(newContent))
                await _statusDiscordMessage.Modify(newContent);
        }
    }
}
