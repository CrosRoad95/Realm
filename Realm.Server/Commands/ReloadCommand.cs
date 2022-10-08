namespace Realm.Server.Commands;

internal class ReloadCommand : ICommand
{
    public string CommandName => "reload";

    private readonly IReloadable[] _reloadable;

    public ReloadCommand(IEnumerable<IReloadable> reloadable)
    {
        _reloadable = reloadable.ToArray();
    }

    public void HandleCommand(string command)
    {
        Console.WriteLine("Reloading server...");
        foreach (var reloadable in _reloadable.OrderBy(x => x.GetPriority()))
            reloadable.Reload();
        Console.WriteLine("Server reloaded");

    }
}
