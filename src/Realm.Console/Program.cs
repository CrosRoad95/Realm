using Realm.Console;
using Realm.Logging;
using Realm.Module.Scripting;
using Realm.Server;
using Realm.Server.Modules;
using Serilog.Events;
using Realm.Configuration;
using Realm.Module.Grpc;
using Realm.Module.Discord;

var console = new ServerConsole();
var logger = new RealmLogger(LogEventLevel.Verbose)
    .AddSeq()
    .GetLogger();

var configurationProvider = new RealmConfigurationProvider();

var builder = new RPGServerBuilder();
builder.AddModule<DiscordModule>();
builder.AddModule<IdentityModule>();
builder.AddModule<ScriptingModule>();
builder.AddModule<ServerScriptingModule>();
builder.AddModule<GrpcModule>();
builder.AddLogger(logger);
builder.AddConsole(console);
builder.AddConfiguration(configurationProvider);

SemaphoreSlim semaphore = new(0);
var server = builder.Build();
Console.CancelKeyPress += async (sender, args) =>
{
    await server.Stop();
    semaphore.Release();
};

await server.Start();
console.Start();
await semaphore.WaitAsync();


