using Realm.Resources.Assets.Interfaces;
using Realm.Tests.Classes;

namespace Realm.Tests.TestServers;

internal class RealmTestingServer : TestingServer
{
    private readonly TestDateTimeProvider _testDateTimeProvider;
    public TestDateTimeProvider TestDateTimeProvider => _testDateTimeProvider;

    public RealmTestingServer(TestDateTimeProvider dateTimeProvider, TestConfigurationProvider testConfigurationProvider, Action<ServiceCollection>? configureServices = null) : base(testConfigurationProvider.GetRequired<SlipeServer.Server.Configuration>("server"), (serverBuilder) =>
    {
        var saveServiceMock = new Mock<ISaveService>(MockBehavior.Strict);
        var rpgServerMock = new Mock<IRPGServer>(MockBehavior.Strict);
        serverBuilder.ConfigureServer(testConfigurationProvider, SlipeServer.Server.ServerBuilders.ServerBuilderDefaultBehaviours.None);
        serverBuilder.ConfigureServices(services =>
        {
            services.AddSingleton(saveServiceMock.Object);
            services.AddSingleton(rpgServerMock.Object);

            services.AddSingleton<IDateTimeProvider>(dateTimeProvider);

            services.AddSingleton<ItemsRegistry>();
            services.AddSingleton<VehicleUpgradeRegistry>();
            services.AddSingleton<LevelsRegistry>();
            services.AddSingleton<IAssetEncryptionProvider, TestAssetEncryptionProvider>();

            services.AddSingleton<IConsole>(new EmptyServerConsole());
            services.AddSingleton<IServerFilesProvider>(new NullServerFilesProvider());
            services.AddLogging(x => x.AddSerilog(new LoggerConfiguration().CreateLogger(), dispose: true));

            services.AddSingleton<IRPGUserManager, RPGUserManager>();
            configureServices?.Invoke(services);
        });
    })
    {
        _testDateTimeProvider = dateTimeProvider;
    }
}
