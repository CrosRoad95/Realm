namespace RealmCore.Console.Utilities;

internal sealed class HotReload
{
    public event Action? OnReload;
    private readonly FileSystemWatcher _fileSystemWatcher;
    private CancellationTokenSource? _changedTaskSource;
    private CancellationToken? _changedTask;

    public HotReload(string path)
    {
        _fileSystemWatcher = new()
        {
            Path = Path.Join(Directory.GetCurrentDirectory(), path),
            NotifyFilter = NotifyFilters.LastWrite,
            IncludeSubdirectories = true,
            Filter = "*.lua"
        };
        _fileSystemWatcher.Changed += OnChanged;
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
        catch (Exception)
        {
            // Ignore
        }
    }
}