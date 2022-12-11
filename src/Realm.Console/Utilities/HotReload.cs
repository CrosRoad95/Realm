using Realm.Server;

namespace Realm.Console.Utilities;

internal class HotReload
{
    public event Action? OnReload;
    private readonly FileSystemWatcher _fileSystemWatcher;
    private CancellationTokenSource? _changedTaskSource;
    private CancellationToken? _changedTask;

    public HotReload(string path)
    {
        _fileSystemWatcher = new FileSystemWatcher();
        _fileSystemWatcher.Path = path;
        _fileSystemWatcher.NotifyFilter = NotifyFilters.FileName | NotifyFilters.Size;
        _fileSystemWatcher.IncludeSubdirectories = true;
        _fileSystemWatcher.Filters.Add("*.js");
        _fileSystemWatcher.Filters.Add("*.ts");
        _fileSystemWatcher.Filters.Add("*.yaml");
        _fileSystemWatcher.Changed += new FileSystemEventHandler(OnChanged);
        _fileSystemWatcher.EnableRaisingEvents = true;
    }

    private async void OnChanged(object source, FileSystemEventArgs e)
    {
        if (_changedTaskSource != null)
            _changedTaskSource.Cancel();

        try
        {
            _changedTaskSource = new();
            _changedTask = _changedTaskSource.Token;
            await Task.Delay(500, _changedTask.Value);
            OnReload?.Invoke();
        }
        catch(Exception)
        {
            // Ignore
        }
    }
}
