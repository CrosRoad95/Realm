namespace Realm.Server.Commands;

internal class TestCommand : ICommand
{
    public string CommandName => "test";
    private readonly ILogger _logger; 
    public TestCommand(ILogger logger)
    {
        _logger = logger.ForContext<TestCommand>();
    }

    public Task HandleCommand(string command)
    {
        _logger.Information("Test command executed");
        return Task.CompletedTask;
    }
}
