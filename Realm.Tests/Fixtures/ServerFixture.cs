namespace Realm.Tests.Fixtures;

public class ServerFixture : IDisposable
{
    internal TestRPGServer TestServer { get; private set; }
    public ServerFixture()
    {
        TestServer = new TestRPGServer();
        TestServer.TestServer.Start();
    }

    public void Dispose()
    {
        TestServer.TestServer.Stop();
    }
}