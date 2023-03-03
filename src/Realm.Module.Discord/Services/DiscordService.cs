using Google.Protobuf;
using Grpc.Net.Client;

namespace Realm.Module.Discord.Services;

internal class DiscordService : IDiscordService
{
    private readonly Messaging.MessagingClient _messagingClient;

    public DiscordService(GrpcChannel grpcChannel)
    {
        _messagingClient = new(grpcChannel);
    }

    public UpdateStatusChannel? UpdateStatusChannel { get; set; }
    public TryConnectAccountChannel? TryConnectAccountChannel { get; set; }
    public PrivateMessageReceived? PrivateMessageReceived { get; set; }

    public async Task<ulong> SendMessage(ulong channelId, string message)
    {
        var response = await _messagingClient.SendMessageAsync(new SendMessageRequest
        {
            ChannelId = channelId,
            Message = message,
        });

        if (response.Success)
            return response.MessageId;

        throw new Exception("Failed to send message");
    }
    
    public async Task<ulong> SendFile(ulong channelId, Stream file, string fileName, string message)
    {
        var byteString = ByteString.FromStream(file);
        var response = await _messagingClient.SendFileAsync(new SendFileRequest
        {
            File = byteString,
            FileName = fileName,
            ChannelId = channelId,
            Message = message,
        });

        if (response.Success)
            return response.MessageId;

        throw new Exception("Failed to send file");
    }

    public async Task<ulong> SendMessageToUser(ulong userId, string message)
    {
        var response = await _messagingClient.SendMessageToUserAsync(new SendMessageToUserRequest
        {
            UserId = userId,
            Message = message,
        });

        if (response.Success)
            return response.MessageId;

        throw new Exception("Failed to send message");
    }
}
