namespace RealmCore.TestingTools;

public class RealmTestingServerHosting<TPlayer> : TestingServerHosting<TPlayer> where TPlayer: RealmPlayer
{
    public TestDateTimeProvider DateTimeProvider => Host.Services.GetRequiredService<TestDateTimeProvider>();
    public TestDebounceFactory DebounceFactory => GetRequiredService<TestDebounceFactory>();

    public RealmTestingServerHosting(Action<HostApplicationBuilder>? outerApplicationBuilder = null, Action<ServerBuilder>? outerServerBuilder = null) : base(new(), hostBuilder =>
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
            ["Identity:Policies:SampleRole:RequireRoles:0"] = "SampleRole",
            ["Identity:Policies:Admin:RequireRoles:0"] = "Admin",
            ["Gameplay:MoneyLimit"] = "1000000",
            ["Gameplay:MoneyPrecision"] = "4",
            ["Gameplay:DefaultInventorySize"] = "20",
            ["Gameplay:AfkCooldown"] = "20",
            ["Gameplay:CurrencyCulture"] = "pl-PL",
            ["Gameplay:Culture"] = "pl-PL",
            ["Database:ConnectionString"] = Environment.GetEnvironmentVariable("RealmCoreTestingDatabaseConnectionString"),
            ["Browser:BaseRemoteUrl"] = "https://localhost:7149",
            ["Assets:Base64Key"] = "ehFQcEzbNPfHt+CKpDJ41Q==",
            ["VehicleLoading:SkipVehicleLoading"] = "true",
            ["Assets:ExcludeExtensions:0"] = ".otf",
            ["Assets:ExcludeExtensions:1"] = ".ttf",
            ["Assets:ExcludeExtensions:2"] = ".txd",
        });

        hostBuilder.Services.AddRealmServerCore(hostBuilder.Configuration);
        outerApplicationBuilder?.Invoke(hostBuilder);

        hostBuilder.Services.AddLogging( builder =>
        {
            builder
                .AddFilter("Microsoft", LogLevel.Warning)
                .AddFilter("System", LogLevel.Warning)
                .AddFilter("RealmCore", LogLevel.Warning)
                .AddConsole();
        });

        hostBuilder.Services.AddSingleton<TestDateTimeProvider>();
        hostBuilder.Services.AddSingleton<IDateTimeProvider>(x => x.GetRequiredService<TestDateTimeProvider>());
        hostBuilder.Services.AddSingleton<TestRealmResourceServer>();
        hostBuilder.Services.AddSingleton<IResourceServer>(x => x.GetRequiredService<TestRealmResourceServer>());
        hostBuilder.Services.AddSingleton<TestDebounceFactory>();
        hostBuilder.Services.AddSingleton<IDebounceFactory>(x => x.GetRequiredService<TestDebounceFactory>());
        hostBuilder.Services.AddHttpClient();
    }, serverBuilder =>
    {
        serverBuilder.AddDefaultServices();
        serverBuilder.AddDefaultLuaMappings();
        serverBuilder.AddResources();
        outerServerBuilder?.Invoke(serverBuilder);
    })
    {
        this.Server.NetWrapperMock.Setup(x => x.GetClientSerialExtraAndVersion(It.IsAny<uint>()))
            .Returns(new Tuple<string, string, string>("A8F5F167F44F4964E6C998DEE827110C", "123", "1.6.0-001"));

        var waitHandle = new AutoResetEvent(false);
        var lifecycle = Host.Services.GetRequiredService<IHostApplicationLifetime>();
        lifecycle.ApplicationStarted.Register(() =>
        {
            waitHandle.Set();
        });

        waitHandle.WaitOne(TimeSpan.FromSeconds(30.0));
    }

    public async Task<RealmPlayer> LoginPlayer(RealmPlayer player, bool dontLoadData = true)
    {
        var userService = player.GetRequiredService<UsersService>();
        var user = await userService.GetUserByUserName(player.Name);

        if (user == null)
        {
            await Host.Services.GetRequiredService<UsersService>().Register(player.Name, "asdASD123!@#");

            user = await userService.GetUserByUserName(player.Name);
            ;
        }

        if (user == null)
            throw new InvalidOperationException();

        var success = await player.GetRequiredService<UsersService>().LogIn(player, user, dontLoadData);

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

    private int _counter = 0;
    public async Task<TPlayer> CreatePlayer(bool notLoggedIn = false, string? name = null, bool dontLoadData = true)
    {
        var tcs = new TaskCompletionSource();
        TPlayer? player = null;
        var playersEventManager = Server.GetRequiredService<PlayersEventManager>();
        playersEventManager.Loaded += HandlePlayersEventManagerLoaded;

        player = Server.AddFakePlayer();
        player.Name = name ?? $"TestPlayer{++_counter}";
        player.Client.FetchSerial();

        void HandlePlayersEventManagerLoaded(Player plr)
        {
            if (plr == player)
                tcs.SetResult();
        }

        var realmResourcesProvider = Server.GetRequiredService<IRealmResourcesProvider>();

        foreach (var resource in realmResourcesProvider.All)
        {
            player.TriggerResourceStarted(resource.NetId);
        }

        player.Browser.RelayReady();

        await tcs.Task;

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
        var vehiclesService = GetRequiredService<VehiclesService>();
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

        if (!await tcs.WaitWithTimeout(TimeSpan.FromSeconds(5)))
            throw new TimeoutException();
    }

    public async Task LogOutPlayer(RealmPlayer player)
    {
        await player.GetRequiredService<UsersService>().LogOut(player);
    }
}

public class RealmTestingServerHosting : RealmTestingServerHosting<RealmTestingPlayer>
{
    public RealmTestingServerHosting(Action<HostApplicationBuilder>? outerApplicationBuilder = null, Action<ServerBuilder>? outerServerBuilder = null) : base(outerApplicationBuilder, outerServerBuilder)
    {

    }
}