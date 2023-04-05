using Grpc.Core;
using Grpc.Net.Client;
using Microsoft.Extensions.Options;

namespace Realm.DiscordBot.Commands;

internal class ConnectUserToServerCommand : InteractionModuleBase<SocketInteractionContext>
{
    private readonly DiscordBotOptions.ConnectServerUserOptions? _connectServerUserOptions;
    private readonly ILogger<ConnectUserToServerCommand> _logger;
    private readonly ConnectUserChannel.ConnectUserChannelClient _connectUserChannelClient;

    public ConnectUserToServerCommand(IOptions<DiscordBotOptions> discordConfiguration, GrpcChannel grpcChannel, ILogger<ConnectUserToServerCommand> logger)
    {
        _connectServerUserOptions = discordConfiguration.Value.ConnectServerUserChannel;
        _logger = logger;
        _connectUserChannelClient = new(grpcChannel);
    }

    [SlashCommand("polaczkonto", "Łączenie konta discord z serwerem mta", false, RunMode.Async)]
    public async Task ConnectUser(string kod)
    {
        if(_connectServerUserOptions == null)
        {
            await RespondAsync("Ten serwer nie wspiera łączenia kont discord z serwerem mta lub konfiguracja serwera jest niepoprawna.", ephemeral: true);
            return;
        }

        if(Context.Channel.Id != _connectServerUserOptions.ChannelId)
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
            var response = await _connectUserChannelClient.TryConnectAsync(new SendConnectionCodeRequest
            {
                Code = kod,
                UserId = Context.User.Id,
            }, deadline: DateTime.UtcNow.AddSeconds(2));
            await RespondAsync(response.Message, ephemeral: true);
        }
        catch (RpcException ex) when (ex.StatusCode == StatusCode.DeadlineExceeded)
        {
            _logger.LogError(ex, "Error while connecting user");
            await RespondAsync("Wystąpił błąd podczas próby połączenia konta, spróbuj ponownie lub skontaktuj się z administracją serwera.", ephemeral: true);
        }
        catch(Exception ex)
        {
            _logger.LogError(ex, "Error while connecting user");
            await RespondAsync("Wystąpił nieznany błąd podczas próby połączenia konta, spróbuj ponownie lub skontaktuj się z administracją serwera.", ephemeral: true);
        }
    }
}
