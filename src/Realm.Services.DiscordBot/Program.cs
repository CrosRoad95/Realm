using Realm.DiscordBot;
using Realm.Configuration;
using Realm.Logging;
using Serilog.Events;
using Grpc.Net.Client;
using Realm.DiscordBot.Stubs;
using Realm.Module.Grpc.Options;
using Discord.Logger;

var realmConfigurationProvider = new RealmConfigurationProvider();
var services = new ServiceCollection();
var realmLogger = new RealmLogger(LogEventLevel.Verbose)
    .AddSeq();

services.AddDiscordLogger();
services.AddLogging(x => x.AddSerilog(realmLogger.GetLogger(), dispose: true));
services.Configure<GrpcOptions>(realmConfigurationProvider.GetSection("Grpc"));
services.Configure<DiscordBotOptions>(realmConfigurationProvider.GetSection("Discord"));
services.AddSingleton<DiscordClient>();
services.AddSingleton(new DiscordSocketClient(new DiscordSocketConfig {
    GatewayIntents = GatewayIntents.All
}));
services.AddSingleton<CommandHandler>();
services.AddSingleton<BotIdProvider>();

services.AddSingleton<GrpcServer>();
services.AddSingleton<MessagingServiceStub>();
services.AddSingleton<TextBasedCommands>();

services.AddSingleton(GrpcChannel.ForAddress("http://localhost:22010"));

// Channels:
services.AddSingleton<DiscordStatusChannel>();
services.AddSingleton<Realm.DiscordBot.Channels.PrivateMessagesChannels>();

var serviceProvider = services.BuildServiceProvider();

var discordIntegration = serviceProvider.GetRequiredService<DiscordClient>();
serviceProvider.GetRequiredService<GrpcServer>();
await discordIntegration.StartAsync();
await Task.Delay(-1);