using Grpc.Core;
using Grpc.Net.Client;

namespace Realm.DiscordBot.Commands;

internal class ConnectAccountToServerCommand : InteractionModuleBase<SocketInteractionContext>
{
    private readonly DiscordBotConfiguration _discordConfiguration;
    private readonly ILogger<ConnectAccountToServerCommand> _logger;
    private readonly ConnectAccountChannel.ConnectAccountChannelClient _connectAccountChannelClient;

    public ConnectAccountToServerCommand(DiscordBotConfiguration discordConfiguration, GrpcChannel grpcChannel, ILogger<ConnectAccountToServerCommand> logger)
    {
        _discordConfiguration = discordConfiguration;
        _logger = logger;
        _connectAccountChannelClient = new(grpcChannel);
    }

    [SlashCommand("polaczkonto", "Łączenie konta discord z serwerem mta", false, RunMode.Async)]
    public async Task ConnectAccount(string kod)
    {
        if(_discordConfiguration.ConnectServerAccountChannel == null)
        {
            await RespondAsync("Ten serwer nie wspiera łączenia kont discord z serwerem mta lub konfiguracja serwera jest niepoprawna.", ephemeral: true);
            return;
        }

        if(Context.Channel.Id != _discordConfiguration.ConnectServerAccountChannel.ChannelId)
        {
            await RespondAsync("Nie możesz użyć tej komendy na tym kanale.", ephemeral: true);
            return;
        }

        if (!Guid.TryParse(kod, out var _))
        {
            await RespondAsync("Kod jest niepoprawny", ephemeral: true);
            return;
        }

        try
        {
            var response = await _connectAccountChannelClient.TryConnectAsync(new SendConnectionCodeRequest
            {
                Code = kod,
                UserId = Context.User.Id,
            }, deadline: DateTime.UtcNow.AddSeconds(2));
            await RespondAsync(response.Message, ephemeral: true);
        }
        catch (RpcException ex) when (ex.StatusCode == StatusCode.DeadlineExceeded)
        {
            _logger.LogError(ex, "Error while connecting account");
            await RespondAsync("Wystąpił błąd podczas próby połączenia konta, spróbuj ponownie lub skontaktuj się z administracją serwera.", ephemeral: true);
        }
        catch(Exception ex)
        {
            _logger.LogError(ex, "Error while connecting account");
            await RespondAsync("Wystąpił nieznany błąd podczas próby połączenia konta, spróbuj ponownie lub skontaktuj się z administracją serwera.", ephemeral: true);
        }
    }
}
