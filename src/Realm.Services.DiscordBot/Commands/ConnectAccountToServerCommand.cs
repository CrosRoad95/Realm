using Grpc.Core;
using Grpc.Net.Client;
using Microsoft.Extensions.Options;

namespace Realm.DiscordBot.Commands;

internal class ConnectAccountToServerCommand : InteractionModuleBase<SocketInteractionContext>
{
    private readonly DiscordBotOptions.ConnectServerAccountOptions? _connectServerAccountOptions;
    private readonly ILogger<ConnectAccountToServerCommand> _logger;
    private readonly ConnectAccountChannel.ConnectAccountChannelClient _connectAccountChannelClient;

    public ConnectAccountToServerCommand(IOptions<DiscordBotOptions> discordConfiguration, GrpcChannel grpcChannel, ILogger<ConnectAccountToServerCommand> logger)
    {
        _connectServerAccountOptions = discordConfiguration.Value.ConnectServerAccountChannel;
        _logger = logger;
        _connectAccountChannelClient = new(grpcChannel);
    }

    [SlashCommand("polaczkonto", "Łączenie konta discord z serwerem mta", false, RunMode.Async)]
    public async Task ConnectAccount(string kod)
    {
        if(_connectServerAccountOptions == null)
        {
            await RespondAsync("Ten serwer nie wspiera łączenia kont discord z serwerem mta lub konfiguracja serwera jest niepoprawna.", ephemeral: true);
            return;
        }

        if(Context.Channel.Id != _connectServerAccountOptions.ChannelId)
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
