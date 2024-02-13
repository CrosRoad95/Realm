using SlipeServer.Server.Resources.Providers;
using SlipeServer.Server.Resources.Interpreters;
using SlipeServer.Server.Resources;
using SlipeServer.Net.Wrappers;
using RealmCore.Tests.Classes;
using RealmCore.Tests.Providers;

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
        yield break;
    }

    public void Refresh()
    {
    }

    public ushort ReserveNetId()
    {
        return 420;
    }
}

internal class RealmTestingServer : TestingServer<RealmTestingPlayer>
{
    public TestDateTimeProvider TestDateTimeProvider => (TestDateTimeProvider)GetRequiredService<IDateTimeProvider>();
    public TestDebounceFactory TestDebounceFactory => (TestDebounceFactory)GetRequiredService<IDebounceFactory>();
    private static string _userName = $"userName{Guid.NewGuid()}";

    protected override IClient CreateClient(uint binaryAddress, INetWrapper netWrapper)
    {
        var player = Instantiate<RealmTestingPlayer>();
        player.Client = new TestingClient(binaryAddress, netWrapper, player);
        return player.Client;
    }

    public RealmTestingServer(TestConfigurationProvider? testConfigurationProvider = null, Action<ServiceCollection>? configureServices = null) : base((testConfigurationProvider ?? new()).GetRequired<SlipeServer.Server.Configuration>("server"), (serverBuilder) =>
    {
        var resourceProvider = new Mock<IResourceProvider>(MockBehavior.Strict);

        resourceProvider.Setup(x => x.Refresh());

        //var saveServiceMock = new Mock<ISaveService>(MockBehavior.Strict);
        //saveServiceMock.Setup(x => x.SaveNewPlayerInventory(It.IsAny<InventoryComponent>(), It.IsAny<int>())).ReturnsAsync(1);
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
            //services.AddSingleton(saveServiceMock.Object);
            services.AddSingleton<TestResourceProvider>();
            services.AddSingleton<IResourceProvider>(x => x.GetRequiredService<TestResourceProvider>());
            services.AddSingleton<IServerFilesProvider, NullServerFilesProvider>();
            services.AddSingleton<IDateTimeProvider, TestDateTimeProvider>();
            services.AddSingleton<IDebounceFactory, TestDebounceFactory>();
            services.AddLogging(x => x.AddSerilog(new LoggerConfiguration().CreateLogger(), dispose: true));

            configureServices?.Invoke(services);
        });
    })
    {
        PlayerJoined += HandlePlayerJoined;
    }

    private void HandlePlayerJoined(RealmTestingPlayer player)
    {
        player.TriggerResourceStarted(420);
    }

    public void ForceUpdate()
    {
        var updateService = GetRequiredService<IPeriodicEventDispatcher>();
        updateService.DispatchEverySecond();
        updateService.DispatchEveryMinute();
    }

    public RealmPlayer CreatePlayer(bool withSerialAndIp = true, string name = "CrosRoad95")
    {
        var player = AddFakePlayer();
        player.Name = name;

        if (withSerialAndIp)
        {
            player.Client = new FakeClient(player)
            {
                Serial = "AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA",
                IPAddress = System.Net.IPAddress.Parse("127.0.0.1")
            };
        }

        GetRequiredService<IPlayerEventManager>().RelayLoaded(player);
        return player;
    }

    public RealmWorldObject CreateObject()
    {
        return GetRequiredService<IElementFactory>().CreateObject(SlipeServer.Server.Enums.ObjectModel.Bin1, Vector3.Zero, Vector3.Zero);
    }

    public FocusableRealmWorldObject CreateFocusableObject()
    {
        return GetRequiredService<IElementFactory>().CreateFocusableObject(SlipeServer.Server.Enums.ObjectModel.Bin1, Vector3.Zero, Vector3.Zero);
    }

    public RealmVehicle CreateVehicle()
    {
        return GetRequiredService<IElementFactory>().CreateVehicle(404, Vector3.Zero, Vector3.Zero);
    }

    public async Task SignInPlayer(RealmPlayer player)
    {
        var userManager = GetRequiredService<UserManager<UserData>>();
        var user = await userManager.GetUserByUserName(player.Name);

        if(user != null)
        {
            // Fix for in memory database
            user.Settings = user.Settings.DistinctBy(x => x.SettingId).ToList();
            user.Bans = user.Bans.DistinctBy(x => x.Id).ToList();
            user.Upgrades = user.Upgrades.DistinctBy(x => x.UpgradeId).ToList();
        }
        if (user == null)
        {
            user = new UserData
            {
                UserName = player.Name,
                Upgrades = new List<UserUpgradeData>(),
                DailyVisits = new(),
            };
            await userManager.CreateAsync(user);
        }
        var success = await player.GetRequiredService<IUsersService>().SignIn(player, user);
        success.Should().BeTrue();
    }
}
