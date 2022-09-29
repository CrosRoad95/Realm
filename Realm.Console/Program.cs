DefaultMtaServer? program = null;
Console.WriteLine("Starting server...");
try
{
    program = new DefaultMtaServer(args);
    program.InitializeScripting("Resources/startup.js");
    program.Start();
}
catch (Exception exception)
{
    if (program != null)
    {
        program.Logger.LogCritical(exception, "{message}", exception.Message);
    }
    else
    {
        Console.WriteLine($"Error in startup {exception.Message}");
    }
    Console.WriteLine("Press any key to exit...");
    Console.Read();
}