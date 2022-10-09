DefaultMtaServer? server = null;

var configuration = new ConfigurationBuilder()
    .AddJsonFile("appsettings.json", false)
    .AddJsonFile("appsettings.development.json", true, true)
    .AddJsonFile("appsettings.local.json", true, true)
    .AddEnvironmentVariables()
    .Build();

Console.WriteLine("Starting server");

var serverConsole = new ServerConsole();

try
{
    server = new DefaultMtaServer(configuration, serverBuilder =>
    {
        serverBuilder.AddGuiFilesLocation("Gui");
        serverBuilder.AddLogic<TestLogic>();
        serverBuilder.ConfigureServices(services =>
        {
            services.AddSingleton<IConsoleCommands>(serverConsole);
        });
    });
    var serverTask = Task.Run(server.Start);
    if(configuration.GetSection("DOTNET_RUNNING_IN_CONTAINER").Value == "true")
    {
        serverTask.Wait();
    }
    else
        serverConsole.Start();
}
catch (Exception exception)
{
    if (server != null)
    {
        //program.Logger.LogCritical(exception, "{message}", exception.Message);
        Console.WriteLine($"Error in startup {exception.Message}");
    }
    else
    {
        Console.WriteLine($"Error in startup {exception.Message}");
    }
    Console.WriteLine("Press any key to exit...");
    Console.Read();
}