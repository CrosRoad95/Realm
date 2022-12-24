using Realm.Resources.AgnosticGuiSystem;

var console = new ServerConsole();
var logger = new RealmLogger(LogEventLevel.Verbose)
    .AddSeq()
    .GetLogger();

var configurationProvider = new RealmConfigurationProvider();

var builder = new RPGServerBuilder();
builder.AddModule<DiscordModule>();
builder.AddModule<IdentityModule>();
builder.AddModule<GrpcModule>();
builder.AddLogger(logger);
builder.AddConsole(console);
builder.AddConfiguration(configurationProvider);

SemaphoreSlim semaphore = new(0);

var server = builder.Build(null, extraBuilderSteps: serverBuilder =>
{
    serverBuilder.AddLogic<PlayerJoinedLogic>();
    serverBuilder.AddLogic<CommandsLogic>();
});

Console.CancelKeyPress += async (sender, args) =>
{
    await server.Stop();
    semaphore.Release();
};


#if DEBUG
var hotReload = new HotReload("../../../Server/Gui");
hotReload.OnReload += async () =>
{
    var stopwatch = Stopwatch.StartNew();
    await server.GetRequiredService<AgnosticGuiSystemService>().UpdateGuiFiles();
    logger.Information("Updated guis in: {time}ms", stopwatch.ElapsedMilliseconds);
};
#endif

await server.Start();
console.Start();
await semaphore.WaitAsync();


