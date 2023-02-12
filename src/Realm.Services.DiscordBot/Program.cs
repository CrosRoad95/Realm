using Realm.DiscordBot;
using Realm.Configuration;
using Microsoft.Extensions.Configuration;
using Realm.Logging;
using Serilog.Events;
using Grpc.Net.Client;

var servicesCollection = new ServiceCollection();
var realmLogger = new RealmLogger(LogEventLevel.Verbose)
    .AddSeq();

servicesCollection.AddSingleton(realmLogger.GetLogger());
servicesCollection.AddSingleton(new RealmConfigurationProvider().Get<DiscordConfiguration>("discord"));
servicesCollection.AddSingleton<DiscordClient>();
servicesCollection.AddSingleton<DiscordClient>();
servicesCollection.AddSingleton(new DiscordSocketClient(new DiscordSocketConfig { GatewayIntents = GatewayIntents.AllUnprivileged | GatewayIntents.GuildMembers }));
servicesCollection.AddSingleton<CommandHandler>();
servicesCollection.AddSingleton<BotIdProvider>();
servicesCollection.AddSingleton(GrpcChannel.ForAddress("http://localhost:22010"));

// Channels:
servicesCollection.AddSingleton<DiscordStatusChannel>();
servicesCollection.AddSingleton<DiscordServerConnectionChannel>();

var serviceProvider = servicesCollection.BuildServiceProvider();

var discordIntegration = serviceProvider.GetRequiredService<DiscordClient>();
await discordIntegration.StartAsync();
await Task.Delay(-1);