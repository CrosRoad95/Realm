using SlipeServer.Server.Resources.Providers;
using SlipeServer.Server.Resources.Interpreters;
using SlipeServer.Server.Resources;
using RealmCore.Resources.Browser;
using RealmCore.Resources.GuiSystem;

namespace RealmCore.Tests.TestServers;

internal class TestResourceProvider : IResourceProvider
{
    public int Resources = 0;
    public TestResourceProvider()
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
        var rpgServerMock = new Mock<IRealmServer>(MockBehavior.Strict);
        var guiSystemServiceMock = new Mock<IGuiSystemService>(MockBehavior.Strict);
        serverBuilder.ConfigureServer(testConfigurationProvider ?? new(), SlipeServer.Server.ServerBuilders.ServerBuilderDefaultBehaviours.None);
        serverBuilder.AddBrowserResource();
        serverBuilder.ConfigureServices(services =>
        {
            services.ConfigureRealmServices();
            services.Configure<AssetsOptions>(options =>
            {
                options.Base64Key = "ehFQcEzbNPfHt+CKpDJ41Q==";
            });
            services.Configure<BrowserOptions>(options =>
            {
                options.Mode = BrowserMode.Local;
                options.BrowserWidth = 1024;
                options.BrowserHeight = 768;
                options.BaseRemoteUrl = "https://localhost:7149";
                options.RequestWhitelistUrl = "localhost";
            });
            services.AddSingleton(saveServiceMock.Object);
            services.AddSingleton(rpgServerMock.Object);
            services.AddSingleton<TestResourceProvider>();
            services.AddSingleton<IResourceProvider>(x => x.GetRequiredService<TestResourceProvider>());
            services.AddSingleton<IServerFilesProvider, NullServerFilesProvider>();
            services.AddSingleton<IDateTimeProvider, TestDateTimeProvider>();
            services.AddLogging(x => x.AddSerilog(new LoggerConfiguration().CreateLogger(), dispose: true));

            configureServices?.Invoke(services);
        });
    })
    {
    }
}
