namespace RealmCore.TestingTools;

public class RealmTestingServer<TPlayer> : TestingServer<TPlayer> where TPlayer: Player
{
    public TestDateTimeProvider DateTimeProvider => (TestDateTimeProvider)GetRequiredService<IDateTimeProvider>();
    public TestDebounceFactory TestDebounceFactory => (TestDebounceFactory)GetRequiredService<IDebounceFactory>();

    protected string _createPlayerName = "";

    protected override IClient CreateClient(uint binaryAddress, INetWrapper netWrapper)
    {
        var player = Instantiate<TPlayer>();
        player.Name = _createPlayerName;
        player.Client = new TestingClient(binaryAddress, netWrapper, player);
        return player.Client;
    }

    public RealmTestingServer(TestConfigurationProvider? testConfigurationProvider = null, Action<ServerBuilder>? configureBuilder = null, Action<ServiceCollection>? configureServices = null) : base((testConfigurationProvider ?? new("")).GetRequired<SlipeServer.Server.Configuration>("server"), (serverBuilder) =>
    {
        var resourceProvider = new Mock<IResourceProvider>(MockBehavior.Strict);

        resourceProvider.Setup(x => x.Refresh());

        //var saveServiceMock = new Mock<ISaveService>(MockBehavior.Strict);
        //saveServiceMock.Setup(x => x.SaveNewPlayerInventory(It.IsAny<InventoryComponent>(), It.IsAny<int>())).ReturnsAsync(1);
        var guiSystemServiceMock = new Mock<IGuiSystemService>(MockBehavior.Strict);
        serverBuilder.ConfigureServer(testConfigurationProvider ?? new(""), ServerBuilderDefaultBehaviours.None);
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

        configureBuilder?.Invoke(serverBuilder);
    })
    {
        PlayerJoined += HandlePlayerJoined;
    }

    private void HandlePlayerJoined(Player player)
    {
        player.TriggerResourceStarted(420);
    }

    public TPlayer CreatePlayer(string name = "FakePlayer")
    {
        _createPlayerName = name;
        var player = AddFakePlayer();
        player.Name = name;

        player.Client = new FakeClient(player)
        {
            Serial = "AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA",
            IPAddress = System.Net.IPAddress.Parse("127.0.0.1")
        };

        GetRequiredService<IPlayersEventManager>().RelayLoaded(player);
        return player;
    }

    public RealmWorldObject CreateObject()
    {
        return GetRequiredService<IElementFactory>().CreateObject(Location.Zero, ObjectModel.Bin1);
    }

    public FocusableRealmWorldObject CreateFocusableObject()
    {
        return GetRequiredService<IElementFactory>().CreateFocusableObject(Location.Zero, ObjectModel.Bin1);
    }

    public RealmVehicle CreateVehicle()
    {
        return GetRequiredService<IElementFactory>().CreateVehicle(Location.Zero, (VehicleModel)404);
    }

    public async Task<RealmPlayer> SignInPlayer(RealmPlayer player)
    {
        var user = await player.GetRequiredService<IPlayerUserService>().GetUserByUserName(player.Name, DateTimeProvider.Now);

        if (user == null)
        {
            await GetRequiredService<IUsersService>().SignUp(player.Name, "asdASD123!@#");

            user = await player.GetRequiredService<IPlayerUserService>().GetUserByUserName(player.Name, DateTimeProvider.Now);
        }

        if (user == null)
            throw new InvalidOperationException();

        var success = await player.GetRequiredService<IUsersService>().SignIn(player, user);
        success.Should().BeTrue();
        return player;
    }

    public async Task<UserData> CreateAccount(string userName)
    {
        var userManager = GetRequiredService<UserManager<UserData>>();
        var user = new UserData
        {
            UserName = userName,
            Upgrades = new List<UserUpgradeData>(),
            DailyVisits = new(),
        };
        await userManager.CreateAsync(user);
        return user;
    }
}

public class RealmTestingServer : RealmTestingServer<RealmTestingPlayer>
{
    public RealmTestingServer(TestConfigurationProvider? testConfigurationProvider = null, Action<ServerBuilder>? configureBuilder = null, Action<ServiceCollection>? configureServices = null) : base(testConfigurationProvider, configureBuilder, configureServices)
    {

    }
}

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
