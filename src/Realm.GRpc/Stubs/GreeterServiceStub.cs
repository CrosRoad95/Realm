namespace Realm.Module.Grpc.Stubs;

internal sealed class GreeterServiceStub : Greeter.GreeterBase
{
    public override Task<HelloReply> SayHello(HelloRequest request, ServerCallContext context)
    {
        return Task.FromResult(new HelloReply
        {
            Message = "hello " + request.Name
        });
    }
}