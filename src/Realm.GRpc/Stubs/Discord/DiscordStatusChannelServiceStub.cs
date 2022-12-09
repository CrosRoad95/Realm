using Discord;

namespace Realm.GRpc.Stubs.Discord;

public sealed class DiscordStatusChannelServiceStub : StatusChannel.StatusChannelBase
{
    private readonly IGrpcDiscord _grpcDiscord;

    public DiscordStatusChannelServiceStub(IGrpcDiscord grpcDiscord)
    {
        _grpcDiscord = grpcDiscord;
    }

    public override async Task<ContentResponse> Update(ContentRequest request, ServerCallContext context)
    {
        if (_grpcDiscord.UpdateStatusChannel == null)
            return new ContentResponse
            {
                Message = "Status serwera nie mógł zostać określony ponieważ moduł 'discord' nie jest dołączony do serwera."
            };

        var newStatus = await _grpcDiscord.UpdateStatusChannel();
        return new ContentResponse
        {
            Message = newStatus
        };
    }
}