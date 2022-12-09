namespace Realm.Server.Commands;

internal class ReloadCommand : ICommand
{
    public string CommandName => "reload";

    private readonly IReloadable[] _reloadable;
    private readonly ILogger _logger;

    public ReloadCommand(IEnumerable<IReloadable> reloadable, ILogger logger)
    {
        _reloadable = reloadable.ToArray();
        _logger = logger.ForContext<ReloadCommand>();
    }

    public void HandleCommand(string command)
    {
        _logger.Information("Reloading server...");
        foreach (var reloadable in _reloadable.OrderBy(x => x.GetPriority()))
            reloadable.Reload();
        _logger.Information("Server reloaded");

    }
}
