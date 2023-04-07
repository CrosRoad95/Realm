using RealmCore.Configuration;
using RealmCore.Logging;
using Serilog.Events;
using Grpc.Net.Client;
using Discord.Logger;
using RealmCore.DiscordBot.Stubs;
using RealmCore.DiscordBot;
using RealmCore.Module.Grpc.Options;

var realmConfigurationProvider = new RealmConfigurationProvider();
var services = new ServiceCollection();
var realmLogger = new RealmLogger("DiscordBot", LogEventLevel.Verbose)
    .AddSeq("http://localhost:5341");

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
services.AddSingleton<RealmCore.DiscordBot.Channels.PrivateMessagesChannels>();

var serviceProvider = services.BuildServiceProvider();

var discordIntegration = serviceProvider.GetRequiredService<DiscordClient>();
serviceProvider.GetRequiredService<GrpcServer>();
await discordIntegration.StartAsync();
await Task.Delay(-1);