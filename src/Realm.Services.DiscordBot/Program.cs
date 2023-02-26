using Realm.DiscordBot;
using Realm.Configuration;
using Realm.Logging;
using Serilog.Events;
using Grpc.Net.Client;
using Realm.DiscordBot.Stubs;

var services = new ServiceCollection();
var realmLogger = new RealmLogger(LogEventLevel.Verbose)
    .AddSeq();

services.AddLogging(x => x.AddSerilog(realmLogger.GetLogger(), dispose: true));
services.AddRealmConfiguration();
services.AddSingleton(x => x.GetRequiredService<IRealmConfigurationProvider>().GetRequired<DiscordBotConfiguration>("Discord"));
services.AddSingleton<DiscordClient>();
services.AddSingleton(new DiscordSocketClient(new DiscordSocketConfig { GatewayIntents = GatewayIntents.AllUnprivileged | GatewayIntents.GuildMembers }));
services.AddSingleton<CommandHandler>();
services.AddSingleton<BotIdProvider>();

services.AddSingleton<GrpcServer>();
services.AddSingleton<MessagingServiceStub>();

services.AddSingleton(GrpcChannel.ForAddress("http://localhost:22010"));

// Channels:
services.AddSingleton<DiscordStatusChannel>();
services.AddSingleton<Realm.DiscordBot.Channels.PrivateMessagesChannels>();

var serviceProvider = services.BuildServiceProvider();

var discordIntegration = serviceProvider.GetRequiredService<DiscordClient>();
serviceProvider.GetRequiredService<GrpcServer>();
await discordIntegration.StartAsync();
await Task.Delay(-1);