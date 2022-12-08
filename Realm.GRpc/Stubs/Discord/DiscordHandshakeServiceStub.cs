using Discord;

namespace Realm.GRpc.Stubs.Discord;

public sealed class DiscordHandshakeServiceStub : Handshake.HandshakeBase
{
    public override Task<HandshakeReply> DoHandshake(HandshakeRequest request, ServerCallContext context)
    {
        return Task.FromResult(new HandshakeReply
        {
            Message = string.Join("", request.Message.Reverse().ToArray())
        });
    }
}