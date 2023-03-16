using Moq;
using Realm.Interfaces.Providers;
using Realm.Interfaces.Server;
using Realm.Server.Console;
using Realm.Server.Extensions;
using Realm.Server.Interfaces;
using Realm.Server.Providers;
using Realm.Server.Services;
using Serilog;

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
