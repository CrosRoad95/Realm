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
});

Console.CancelKeyPress += async (sender, args) =>
{
    await server.Stop();
    semaphore.Release();
};

await server.Start();
console.Start();
await semaphore.WaitAsync();


