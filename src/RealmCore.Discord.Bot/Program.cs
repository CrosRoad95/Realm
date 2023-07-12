using Microsoft.Extensions.DependencyInjection;
using RealmCore.Configuration;
using RealmCore.Logging;
using RealmCore.Discord.Logger;
using Serilog;
using Serilog.Events;
using RealmCore.Discord.Integration.Channels;
using RealmCore.Discord.Integration.Extensions;
using RealmCore.Discord.Integration.Interfaces;
using Discord.WebSocket;

var realmConfigurationProvider = new RealmConfigurationProvider();
var services = new ServiceCollection();
var realmLogger = new RealmLogger("DiscordBot", LogEventLevel.Verbose);
realmLogger.LoggerConfiguration.WriteTo.Seq("http://localhost:5341");

services.AddDiscordLogger();
services.AddLogging(x => x.AddSerilog(realmLogger.GetLogger(), dispose: true));

services.AddDiscordChannel<DiscordStatusChannel>();
services.AddDiscordChannel<PrivateMessagesChannels>();

services.AddRealmDiscordBotIntegration(realmConfigurationProvider);

var serviceProvider = services.BuildServiceProvider();

var discordIntegration = serviceProvider.GetRequiredService<IRealmDiscordClient>();
discordIntegration.Ready += () =>
{
    var discordLogger = serviceProvider.GetRequiredService<IDiscordLogger>();
    var client = serviceProvider.GetRequiredService<DiscordSocketClient>();
    discordLogger.Attach(client);
};
await discordIntegration.StartAsync();
await Task.Delay(-1);