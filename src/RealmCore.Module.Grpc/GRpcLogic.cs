namespace RealmCore.Module.Grpc;

public class GRpcLogic
{
    public GRpcLogic(Server server)
    {
        server.Start();
    }
}
