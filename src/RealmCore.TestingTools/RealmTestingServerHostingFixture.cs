namespace RealmCore.TestingTools;

public class RealmTestingServerHostingFixture : IDisposable
{
    private readonly RealmTestingServerHosting _hosting;

    public RealmTestingServerHosting Hosting => _hosting;

    public RealmTestingServerHostingFixture()
    {
        _hosting = new RealmTestingServerHosting();
    }

    public void Dispose()
    {
        _hosting.Dispose();
    }
}
