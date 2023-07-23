using Grpc.Core;
using WebAdminPanel;

namespace RealmCore.Web.AdminPanel.Stubs;

public class HandshakeServiceStub : Handshake.HandshakeClient
{
    public override AsyncUnaryCall<HandshakeReply> DoHandshakeAsync(HandshakeRequest request, CallOptions options)
    {
        return base.DoHandshakeAsync(request, options);
    }
}
