using Microsoft.Extensions.Hosting;
using SlipeServer.Server;
using System.IO.Compression;
using System.Net;
using System.Net.Http.Headers;
using RealmCore.WebHosting;

namespace RealmCore.TestingTools;

public class PAttach120DelegatingHandler : DelegatingHandler
{
    protected override async Task<HttpResponseMessage> SendAsync(
      HttpRequestMessage request, CancellationToken cancellationToken)
    {
        if(request.RequestUri?.ToString() == "https://github.com/Patrick2562/mtasa-pAttach/releases/download/v1.2.0/pAttach-v1.2.0.zip")
        {
            string content = "<meta><script src=\"client.lua\" type=\"client\" /><export function=\"attach\" type=\"shared\" /></meta>";

            using var memoryStream = new MemoryStream();
            using (var archive = new ZipArchive(memoryStream, ZipArchiveMode.Create, true))
            {

                {
                    var entry = archive.CreateEntry("pAttach/meta.xml");
                    using var writer = new StreamWriter(entry.Open());
                    await writer.WriteAsync(content);
                }

                {
                    var entry = archive.CreateEntry("pAttach/client.lua");
                    using var writer = new StreamWriter(entry.Open());
                    await writer.WriteAsync(Guid.NewGuid().ToString()); // Write random data to invalidate potential cache
                }
            }

            memoryStream.Seek(0, SeekOrigin.Begin);
            HttpResponseMessage response = new HttpResponseMessage(HttpStatusCode.OK);
            var data = memoryStream.ToArray();
            response.Content = new ByteArrayContent(memoryStream.ToArray());
            response.Content.Headers.ContentType = new MediaTypeHeaderValue("application/zip");
            response.Content.Headers.ContentDisposition = new ContentDispositionHeaderValue("attachment")
            {
                FileName = "archive.zip"
            };

            return response;
        }

        return await base.SendAsync(request, cancellationToken);
    }
}

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

    public RealmTestingServer(TestConfigurationProvider? testConfigurationProvider = null, Action<ServerBuilder>? configureBuilder = null, Action<IServiceCollection>? configureServices = null) : base((testConfigurationProvider ?? new("")).GetValue<Configuration>("server"), (serverBuilder) =>
    {
        var resourceProvider = new Mock<IResourceProvider>(MockBehavior.Strict);
        var httpClient = new HttpClient(new PAttach120DelegatingHandler());
        resourceProvider.Setup(x => x.Refresh());

        //var saveServiceMock = new Mock<ISaveService>(MockBehavior.Strict);
        //saveServiceMock.Setup(x => x.SaveNewPlayerInventory(It.IsAny<InventoryComponent>(), It.IsAny<int>())).ReturnsAsync(1);
        var guiSystemServiceMock = new Mock<IGuiSystemService>(MockBehavior.Strict);
        serverBuilder.ConfigureServer(testConfigurationProvider ?? new(""), ServerBuilderDefaultBehaviours.None, excludeResources: ExcludeResources.BoneAttach);
        serverBuilder.ConfigureServices(services =>
        {
            services.AddSingleton(httpClient);
            services.ConfigureRealmServices(testConfigurationProvider ?? new(""));
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

        GetRequiredService<PlayersEventManager>().RelayLoaded(player);
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

    public async Task SignOutPlayer(RealmPlayer player)
    {
        await player.GetRequiredService<IUsersService>().SignOut(player);
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
    public RealmTestingServer(TestConfigurationProvider? testConfigurationProvider = null, Action<ServerBuilder>? configureBuilder = null, Action<IServiceCollection>? configureServices = null) : base(testConfigurationProvider, configureBuilder, configureServices)
    {

    }
}

public class NewRealmTestingServer
{
    private readonly IHost _host;

    public NewRealmTestingServer()
    {
        var builder = Host.CreateApplicationBuilder();
        builder.Configuration.AddInMemoryCollection(new Dictionary<string, string?>
        {
            ["Server:IsVoiceEnabled"] = "true",
            ["Server:ServerName"] = "Default New-RealmCore Test server",
            ["Server:Port"] = "22003",
            ["Server:HttpPort"] = "22005",
            ["Server:MaxPlayerCount"] = "128",
            ["ServerList:GameType"] = "",
            ["ServerList:MapName"] = "",
            ["Discord:Token"] = "true",
            ["Discord:Guild"] = "997787973775011850",
            ["Discord:StatusChannel:ChannelId"] = "1025774028255932497",
            ["Identity:Policies:Admin:RequireRoles:0"] = "Admin",
            //["Identity:Policies:Admin:RequireClaims:Test"] = "true",
            ["Gameplay:MoneyLimit"] = "1000000",
            ["Gameplay:MoneyPrecision"] = "4",
            ["Gameplay:DefaultInventorySize"] = "20",
            ["Gameplay:AfkCooldown"] = "20",
            ["Database:ConnectionString"] = "asd"
        });
        builder.Services.AddRealmServer<RealmTestingPlayer>(builder.Configuration, builder =>
        {

        });

        _host = builder.Build();
    }

    public RealmTestingPlayer CreatePlayer()
    {
        var server = _host.Services.GetRequiredService<MtaServer>();

        return null; // TODO: !!!
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
