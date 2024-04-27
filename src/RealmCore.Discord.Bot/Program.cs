using Microsoft.Extensions.DependencyInjection;
using RealmCore.Configuration;
using RealmCore.Logging;
using Serilog;
using Serilog.Events;
using RealmCore.Discord.Integration.Channels;
using RealmCore.Discord.Integration.Extensions;
using RealmCore.Discord.Integration.Interfaces;
using Discord.WebSocket;

var realmConfigurationProvider = new RealmConfiguration();
var services = new ServiceCollection();
var realmLogger = new RealmLogger("DiscordBot", LogEventLevel.Verbose);

services.AddLogging(x => x.AddSerilog(realmLogger.GetLogger(), dispose: true));

services.AddDiscordChannel<DiscordStatusChannel>();
services.AddDiscordChannel<PrivateMessagesChannels>();

services.AddRealmDiscordBotIntegration(realmConfigurationProvider);

var serviceProvider = services.BuildServiceProvider();

var discordIntegration = serviceProvider.GetRequiredService<IRealmDiscordClient>();

await discordIntegration.StartAsync();
await Task.Delay(-1);