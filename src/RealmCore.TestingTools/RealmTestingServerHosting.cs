namespace RealmCore.TestingTools;

public class RealmTestingServer2<TPlayer> : TestingServer<TPlayer> where TPlayer : Player
{
    private int _playerCounter = 0;
    public RealmTestingServer2(IServiceProvider serviceProvider, Configuration? configuration = null) : base(serviceProvider, configuration)
    {
        this.NetWrapperMock.Setup(x => x.GetClientSerialExtraAndVersion(It.IsAny<uint>()))
            .Returns(new Tuple<string, string, string>("7815696ECBF1C96E6894B779456D330E", "", ""));
    }

    protected override IClient CreateClient(uint binaryAddress, INetWrapper netWrapper)
    {
        var player = Instantiate<TPlayer>();
        player.Name = $"TestPlayer{++_playerCounter}"; // TODO:
        var client = new TestingClient(binaryAddress, netWrapper, player);
        client.FetchSerial();
        player.Client = client;
        return client;
    }

}

public class RealmTestingServerHosting : TestingServerHosting<RealmTestingPlayer>
{
    public TestDateTimeProvider DateTimeProvider => Host.Services.GetRequiredService<TestDateTimeProvider>();
    public TestDebounceFactory TestDebounceFactory => (TestDebounceFactory)GetRequiredService<IDebounceFactory>();

    public RealmTestingServerHosting(Action<HostApplicationBuilder>? outerApplicationBuilder = null, Action<ServerBuilder>? outerServerBuilder = null) : base(new(), services => new RealmTestingServer2<RealmTestingPlayer>(services, new()), hostBuilder =>
    {
        hostBuilder.Configuration.AddInMemoryCollection(new Dictionary<string, string?>
        {
            ["Server:IsVoiceEnabled"] = "true",
            ["Server:ServerName"] = "Default New-RealmCore Test server",
            ["Server:Port"] = "22300",
            ["Server:HttpPort"] = "22301",
            ["Server:MaxPlayerCount"] = "128",
            ["ServerList:GameType"] = "",
            ["ServerList:MapName"] = "",
            ["Discord:Token"] = "true",
            ["Discord:Guild"] = "997787973775011850",
            ["Discord:StatusChannel:ChannelId"] = "1025774028255932497",
            ["Identity:Policies:SampleRole:RequireRoles:0"] = "SampleRole",
            ["Identity:Policies:Admin:RequireRoles:0"] = "Admin",
            //["Identity:Policies:Admin:RequireClaims:Test"] = "true",
            ["Gameplay:MoneyLimit"] = "1000000",
            ["Gameplay:MoneyPrecision"] = "4",
            ["Gameplay:DefaultInventorySize"] = "20",
            ["Gameplay:AfkCooldown"] = "20",
            ["Gameplay:CurrencyCulture"] = "pl-PL",
            ["Gameplay:Culture"] = "pl-PL",
            ["Database:ConnectionString"] = Environment.GetEnvironmentVariable("RealmCoreTestingDatabaseConnectionString"),
            ["Browser:BaseRemoteUrl"] = "https://localhost:7149",
            ["Assets:Base64Key"] = "ehFQcEzbNPfHt+CKpDJ41Q=="
        });

        hostBuilder.Services.AddRealmServerCore(hostBuilder.Configuration);
        outerApplicationBuilder?.Invoke(hostBuilder);

        hostBuilder.Services.AddSingleton<TestDateTimeProvider>();
        hostBuilder.Services.AddSingleton<IDateTimeProvider>(x => x.GetRequiredService<TestDateTimeProvider>());
        hostBuilder.Services.AddSingleton<TestRealmResourceServer>();
        hostBuilder.Services.AddSingleton<IResourceServer>(x => x.GetRequiredService<TestRealmResourceServer>());
        hostBuilder.Services.AddSingleton<TestDebounceFactory>();
        hostBuilder.Services.AddSingleton<IDebounceFactory>(x => x.GetRequiredService<TestDebounceFactory>());
        hostBuilder.Services.AddHttpClient();
    }, serverBuilder =>
    {
        serverBuilder.AddDefaultLuaMappings();
        serverBuilder.AddResources();
        outerServerBuilder?.Invoke(serverBuilder);
    })
    {
        var waitHandle = new AutoResetEvent(false);
        var lifecycle = Host.Services.GetRequiredService<IHostApplicationLifetime>();
        lifecycle.ApplicationStarted.Register(() =>
        {
            waitHandle.Set();
        });

        waitHandle.WaitOne(TimeSpan.FromSeconds(30.0));
    }

    public T GetRequiredService<T>() where T: class => Host.Services.GetRequiredService<T>();

    public async Task<RealmPlayer> LoginPlayer(RealmPlayer player, bool dontLoadData = true)
    {
        var user = await player.GetRequiredService<IPlayerUserService>().GetUserByUserName(player.Name, DateTimeProvider.Now);

        if (user == null)
        {
            await Host.Services.GetRequiredService<IUsersService>().Register(player.Name, "asdASD123!@#");

            user = await player.GetRequiredService<IPlayerUserService>().GetUserByUserName(player.Name, DateTimeProvider.Now);
        }

        if (user == null)
            throw new InvalidOperationException();

        var success = await player.GetRequiredService<IUsersService>().LogIn(player, user, dontLoadData);
        success.Switch(loggedIn =>
        {
            // Ok
        }, userDisabled =>
        {
            throw new XunitException("User should not be disabled");
        }, playerAlreadyLoggedIn =>
        {
            throw new XunitException("Player should not be already logged in");
        }, userAlreadyInUse =>
        {
            throw new XunitException("Player should not be already in use");
        });

        return player;
    }

    public async Task<RealmTestingPlayer> CreatePlayer(bool notLoggedIn = false, string? name = null, bool dontLoadData = true)
    {
        var player = Server.AddFakePlayer();
        if (name != null)
            player.Name = name;

        var testRealmResourceServer = Server.GetRequiredService<TestRealmResourceServer>();

        foreach (var resource in testRealmResourceServer.Resources)
        {
            player.TriggerResourceStarted(resource.NetId);
        }

        if (!notLoggedIn)
        {
            await LoginPlayer(player, dontLoadData);
        }

        return player;
    }

    public RealmVehicle CreateVehicle()
    {
        return GetRequiredService<IElementFactory>().CreateVehicle(Location.Zero, (VehicleModel)404);
    }

    public RealmWorldObject CreateObject()
    {
        return GetRequiredService<IElementFactory>().CreateObject(Location.Zero, ObjectModel.Bin1);
    }

    public FocusableRealmWorldObject CreateFocusableObject()
    {
        return GetRequiredService<IElementFactory>().CreateFocusableObject(Location.Zero, ObjectModel.Bin1);
    }

    public async Task<RealmVehicle> CreatePersistentVehicle()
    {
        var vehiclesService = GetRequiredService<IVehiclesService>();
        var vehicle = await vehiclesService.CreatePersistantVehicle(Location.Zero, (VehicleModel)404);
        if (vehicle == null)
            throw new NullReferenceException("Vehicle is null");

        vehicle.Persistence.Id.Should().NotBe(0);
        return vehicle;
    }

    public async Task DisconnectPlayer(RealmPlayer player)
    {
        var tcs = new TaskCompletionSource();
        void handlePlayerDisposed(Element element)
        {
            tcs.SetResult();
        }

        player.Disposed += handlePlayerDisposed;
        player.TriggerDisconnected(QuitReason.Quit);

        await tcs.Task;
    }


    public async Task LogOutPlayer(RealmPlayer player)
    {
        await player.GetRequiredService<IUsersService>().LogOut(player);
    }
}

public sealed class TestRealmResourceServer : IResourceServer
{
    public List<Resource> Resources = [];
    public void AddAdditionalResource(Resource resource, Dictionary<string, byte[]> files)
    {
        RealmResourceServer._resourceCounter++;
        Resources.Add(resource);
    }

    public void RemoveAdditionalResource(Resource resource)
    {
        RealmResourceServer._resourceCounter--;
    }

    public void Start() { }

    public void Stop() { }
}
