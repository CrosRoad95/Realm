namespace Realm.Server.Commands;

internal class ReloadCommand : ICommand
{
    public string CommandName => "reload";

    private readonly Interfaces.IInternalRPGServer _rpgServer;
    private readonly ILogger _logger;

    public ReloadCommand(Interfaces.IInternalRPGServer rpgServer, ILogger logger)
    {
        _rpgServer = rpgServer;
        _logger = logger.ForContext<ReloadCommand>();
    }

    public async void HandleCommand(string command)
    {
        _logger.Information("Reloading server...");
        await _rpgServer.DoReload();
        _logger.Information("Server reloaded");
    }
}
