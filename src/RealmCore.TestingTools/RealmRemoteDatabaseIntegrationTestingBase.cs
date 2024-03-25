namespace RealmCore.TestingTools;

public abstract class RealmRemoteDatabaseIntegrationTestingBase<TRealmTestingServer, TRealmPlayer> : RealmIntegrationTestingBase<TRealmTestingServer, TRealmPlayer>
    where TRealmTestingServer : RealmTestingServer<TRealmPlayer>
    where TRealmPlayer : Player
{
    protected abstract string DatabaseName { get; }

    protected override string GetConnectionString()
    {
        var connectionString = Environment.GetEnvironmentVariable("RealmCoreTestingDatabaseConnectionString");
        return connectionString ?? throw new InvalidOperationException("Connection string environment variable 'RealmCoreTestingDatabaseConnectionString' not found or set");
    }
}

public abstract class RealmRemoteDatabaseIntegrationTestingBase : RealmRemoteDatabaseIntegrationTestingBase<RealmTestingServer, RealmTestingPlayer>
{
    protected override RealmTestingServer CreateServer(TestConfigurationProvider? cnofiguration = null, Action<ServerBuilder>? configureBuilder = null, Action<ServiceCollection>? configureServices = null)
    {
        _server ??= new RealmTestingServer(cnofiguration ?? new TestConfigurationProvider(""), configureBuilder, services =>
        {
            services.AddRealmTestingServices(true);
            configureServices?.Invoke(services);
        });
        return _server;
    }
}