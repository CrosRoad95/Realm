using Serilog;
using Serilog.Core;
using Serilog.Events;

namespace RealmCore.TestingTools;

public class TestingServerHosting2<T> : IDisposable where T : Player
{
    private readonly IHost host;

    public TestingServer<T> Server { get; }
    public IHost Host => this.host;

    public TestingServerHosting2(Configuration configuration, Func<IServiceProvider, TestingServer<T>> serverFactory, Action<HostApplicationBuilder>? applicationBuilder = null, Action<ServerBuilder>? serverBuilder = null)
    {
        var builder = Microsoft.Extensions.Hosting.Host.CreateApplicationBuilder();

        builder.Services.AddMtaServer<TestingServer<T>>(configuration, serverFactory, builder =>
        {
            builder.AddDefaultServices();
            serverBuilder?.Invoke(builder);
        });

        applicationBuilder?.Invoke(builder);
        this.host = builder.Build();

        var tcs = new TaskCompletionSource();
        var lifecycle = Host.Services.GetRequiredService<IHostApplicationLifetime>();
        lifecycle.ApplicationStarted.Register(() =>
        {
           tcs.SetResult();
        });

        var _ = Task.Run(async () =>
        {
            try
            {
                await this.host.RunAsync();
            }
            catch(Exception ex)
            {
                tcs.SetException(ex);
            }
        });

        if (!tcs.WaitWithTimeout(TimeSpan.FromSeconds(60)).Result)
        {
            throw new XunitException("Testing hosting failed to start.");
        }

        tcs.Task.Wait();

        this.Server = this.host.Services.GetRequiredService<TestingServer<T>>();
    }

    public void Dispose()
    {
        var waitHandle = new AutoResetEvent(false);
        this.Server.Stopped += () =>
        {
            waitHandle.Set();
        };
        this.host.StopAsync().Wait();
        waitHandle.WaitOne(TimeSpan.FromSeconds(30));
    }
}

public class TestingServerHosting2 : TestingServerHosting2<TestingPlayer>
{
    public TestingServerHosting2(Configuration configuration, Action<HostApplicationBuilder>? applicationBuilder = null, Action<ServerBuilder>? serverBuilder = null) : base(configuration, services => new TestingServer<TestingPlayer>(services, configuration), applicationBuilder, serverBuilder)
    {

    }
}

public class RealmTestingServerHosting : TestingServerHosting2<RealmTestingPlayer>
{
    public TestDateTimeProvider DateTimeProvider => Host.Services.GetRequiredService<TestDateTimeProvider>();
    public TestDebounceFactory TestDebounceFactory => (TestDebounceFactory)GetRequiredService<IDebounceFactory>();

    public RealmTestingServerHosting(Action<HostApplicationBuilder>? outerApplicationBuilder = null, Action<ServerBuilder>? outerServerBuilder = null) : base(new(), services => new RealmTestingServer<RealmTestingPlayer>(services, new()), hostBuilder =>
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


        var levelSwitch = new LoggingLevelSwitch(LogEventLevel.Warning);
        var logger = new LoggerConfiguration()
            .MinimumLevel.Override("Microsoft.EntityFrameworkCore.Database.Command", LogEventLevel.Warning)
            .MinimumLevel.Override("Microsoft.AspNetCore", LogEventLevel.Warning)
            .MinimumLevel.Override("Microsoft.AspNetCore.Components", LogEventLevel.Warning)
            .MinimumLevel.ControlledBy(levelSwitch);

        hostBuilder.Services.AddLogging(x => x.AddSerilog(logger.CreateLogger(), dispose: true));

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
        var user = await player.GetRequiredService<IPlayerUserService>().GetUserByUserName(player.Name);

        if (user == null)
        {
            await Host.Services.GetRequiredService<IUsersService>().Register(player.Name, "asdASD123!@#");

            user = await player.GetRequiredService<IPlayerUserService>().GetUserByUserName(player.Name);
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
