var builder = new RPGServerBuilder();
builder.AddDefaultModules()
    .AddDefaultLogger()
    .AddDefaultConsole()
    .AddDefaultConfiguration();

SemaphoreSlim semaphore = new(0);

var server = builder.Build(null, extraBuilderSteps: serverBuilder =>
{
    serverBuilder.AddLogic<PlayerJoinedLogic>();
    serverBuilder.AddLogic<CommandsLogic>();
    serverBuilder.AddLogic<SamplePickupsLogic>();
    serverBuilder.AddLogic<PlayerBindsLogic>();
    serverBuilder.AddLogic<ItemsLogic>();
    serverBuilder.AddLogic<VehicleUpgradesLogic>();
    serverBuilder.AddLogic<AchievementsLogic>();
    serverBuilder.AddLogic<WorldLogic>();
    serverBuilder.AddLogic<LevelsLogic>();
#if DEBUG
    serverBuilder.AddLogic<HotReloadLogic>("../../../Server/Gui");
#endif
});

Console.CancelKeyPress += (sender, args) =>
{
    try
    {
        server.Stop().Wait();
    }
    finally
    {
        semaphore.Release();
    }
};

await server.Start();
server.GetRequiredService<IConsole>().Start();
await semaphore.WaitAsync();
Console.WriteLine("Server stopped.");