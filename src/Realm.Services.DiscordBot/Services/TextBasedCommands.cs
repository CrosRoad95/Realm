using Grpc.Core;
using Grpc.Net.Client;

namespace Realm.DiscordBot.Services;

internal class TextBasedCommands
{
    private readonly Discord.Commands.CommandsClient _commandsClient;
    public TextBasedCommands(GrpcChannel grpcChannel)
    {
        _commandsClient = new(grpcChannel);
    }

    public async Task Relay(SocketMessage socketMessage)
    {
        await _commandsClient.SendTextBasedCommandAsync(new SendTextBasedCommandRequest
        {
            ChannelId = socketMessage.Channel.Id,
            Command = socketMessage.Content,
            UserId = socketMessage.Author.Id
        });
    }
}
