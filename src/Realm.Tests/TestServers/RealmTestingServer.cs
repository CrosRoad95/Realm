using Moq;
using Realm.Domain.Registries;
using Realm.Interfaces.Providers;
using Realm.Interfaces.Server;
using Realm.Server.Console;
using Realm.Server.Extensions;
using Realm.Server.Interfaces;
using Realm.Server.Providers;
using Serilog;

namespace Realm.Tests.TestServers;

internal class RealmTestingServer : TestingServer
{
    public RealmTestingServer() : base(null, (serverBuilder) =>
    {
        var saveServiceMock = new Mock<ISaveService>(MockBehavior.Strict);
        var rpgServerMock = new Mock<IRPGServer>(MockBehavior.Strict);
        serverBuilder.ConfigureServer(new TestConfigurationProvider());
        serverBuilder.ConfigureServices(services =>
        {
            services.AddSingleton(saveServiceMock.Object);
            services.AddSingleton(rpgServerMock.Object);

            services.AddSingleton<ItemsRegistry>();
            services.AddSingleton<VehicleUpgradeRegistry>();
            services.AddSingleton<LevelsRegistry>();

            services.AddSingleton<IConsole>(new EmptyServerConsole());
            services.AddSingleton<IServerFilesProvider>(new NullServerFilesProvider());
            services.AddSingleton<ILogger>(new LoggerConfiguration().CreateLogger());
        });
    })
    {
    }
}
