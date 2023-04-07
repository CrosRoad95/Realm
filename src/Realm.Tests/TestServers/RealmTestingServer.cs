using Realm.Resources.Assets.Interfaces;
using Realm.Server.Security.Interfaces;
using Realm.Server.Security;
using Realm.Tests.Classes;
using SlipeServer.Server.Resources.Providers;
using Grpc.Core;
using SlipeServer.Server.Resources.Interpreters;
using SlipeServer.Server.Resources;
using SlipeServer.Server.ElementCollections;
using SlipeServer.Server.Elements;
using Microsoft.Extensions.DependencyInjection;

namespace Realm.Tests.TestServers;

internal class TestResourcePRovider : IResourceProvider
{
    public int Resources = 0;
    public TestResourcePRovider()
    {
    }

    public void AddResourceInterpreter(IResourceInterpreter resourceInterpreter)
    {
        throw new NotImplementedException();
    }

    public byte[] GetFileContent(string resource, string file)
    {
        throw new NotImplementedException();
    }

    public IEnumerable<string> GetFilesForResource(string name)
    {
        throw new NotImplementedException();
    }

    public Resource GetResource(string name)
    {
        throw new NotImplementedException();
    }

    public IEnumerable<Resource> GetResources()
    {
        throw new NotImplementedException();
    }

    public void Refresh()
    {
    }

    public ushort ReserveNetId()
    {
        return 420;
    }
}

internal class RealmTestingServer : TestingServer
{
    public TestDateTimeProvider TestDateTimeProvider => GetRequiredService<TestDateTimeProvider>();

    public RealmTestingServer(TestConfigurationProvider? testConfigurationProvider = null, Action<ServiceCollection>? configureServices = null) : base((testConfigurationProvider ?? new()).GetRequired<SlipeServer.Server.Configuration>("server"), (serverBuilder) =>
    {
        var resourceProvider = new Mock<IResourceProvider>(MockBehavior.Strict);

        resourceProvider.Setup(x => x.Refresh());

        var saveServiceMock = new Mock<ISaveService>(MockBehavior.Strict);
        saveServiceMock.Setup(x => x.SaveNewPlayerInventory(It.IsAny<InventoryComponent>(), It.IsAny<int>())).ReturnsAsync(1);
        var rpgServerMock = new Mock<IRPGServer>(MockBehavior.Strict);
        serverBuilder.ConfigureServer(testConfigurationProvider ?? new(), SlipeServer.Server.ServerBuilders.ServerBuilderDefaultBehaviours.None);
        serverBuilder.ConfigureServices(services =>
        {

            services.AddSingleton(saveServiceMock.Object);
            services.AddSingleton(rpgServerMock.Object);
            services.AddSingleton<TestResourcePRovider>();
            services.AddSingleton<IResourceProvider>(x => x.GetRequiredService<TestResourcePRovider>());

            services.AddSingleton<IDateTimeProvider, TestDateTimeProvider>();

            services.AddSingleton<IActiveUsers, ActiveUsers>();

            services.AddSingleton<ItemsRegistry>();
            services.AddSingleton<VehicleUpgradeRegistry>();
            services.AddSingleton<LevelsRegistry>();
            services.AddSingleton<IAssetEncryptionProvider, TestAssetEncryptionProvider>();

            services.AddSingleton<IConsole>(new EmptyServerConsole());
            services.AddSingleton<IServerFilesProvider>(new NullServerFilesProvider());
            services.AddLogging(x => x.AddSerilog(new LoggerConfiguration().CreateLogger(), dispose: true));

            services.AddSingleton<IUsersService, UsersService>();
            configureServices?.Invoke(services);
        });
    })
    {
    }
}
