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