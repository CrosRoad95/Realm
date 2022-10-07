namespace Realm.Tests.Fixtures;

public class ServerFixture : IDisposable
{
    private readonly EventWaitHandle waitHandle = new(false, EventResetMode.AutoReset);
    internal DefaultTestServer TestServer { get; private set; }
    public ServerFixture()
    {
        TestServer = new DefaultTestServer();
        Task.Run(() =>
        {
            TestServer.TestServer.Start();
            waitHandle.WaitOne();
        });
    }

    public void Dispose()
    {
        TestServer.TestServer.Stop();
        waitHandle.Set();
    }
}