using Microsoft.Extensions.DependencyInjection;
using RealmCore.Logging;
using Serilog;
using Serilog.Events;
using RealmCore.Discord.Integration.Channels;
using RealmCore.Discord.Integration.Extensions;
using Discord.WebSocket;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Configuration;
using Discord;
using System.Reflection;

var app = Host.CreateDefaultBuilder(args)
    .ConfigureAppConfiguration((hostingContext, builder) =>
    {
        var configuration = new ConfigurationBuilder()
            .AddUserSecrets(Assembly.GetExecutingAssembly())
            .AddEnvironmentVariables()
            .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
            .AddJsonFile("appsettings.Local.json", optional: true, reloadOnChange: true)
            .AddJsonFile($"appsettings.{hostingContext.HostingEnvironment.EnvironmentName}.json", optional: true)
            .Build();

        builder.Sources.Clear();
        builder.AddConfiguration(configuration);
    })
    .ConfigureServices((hostingContext, services) =>
    {
        var realmLogger = new RealmLogger("SampleDiscordBot", LogEventLevel.Verbose);

        services.AddLogging(x => x.AddSerilog(realmLogger.GetLogger(), dispose: true));

        services.AddSingleton(new DiscordSocketClient(new DiscordSocketConfig
        {
            GatewayIntents = GatewayIntents.All
        }));

        services.AddDiscordChannel<DiscordStatusChannel>();
        services.AddDiscordChannel<DiscordPrivateMessagesChannels>();

        var configuration = hostingContext.Configuration;
        services.AddRealmDiscordBotIntegration(configuration);

    })
    .Build();

await app.RunAsync();