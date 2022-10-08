namespace Realm.Tests.Fixtures;

public class ServerFixture : IDisposable
{
    internal DefaultTestServer TestServer { get; private set; }
    public ServerFixture()
    {
        TestServer = new DefaultTestServer();
        TestServer.TestServer.Start();
    }

    public void Dispose()
    {
        TestServer.TestServer.Stop();
    }
}