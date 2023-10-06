namespace RealmCore.Console.Utilities;

internal sealed class HotReload
{
    public event Action? OnReload;
    private readonly FileSystemWatcher _fileSystemWatcher;
    private readonly Debounce _debounce;

    public HotReload(string path)
    {
        _fileSystemWatcher = new()
        {
            Path = Path.Join(Directory.GetCurrentDirectory(), path),
            NotifyFilter = NotifyFilters.LastWrite,
            IncludeSubdirectories = true,
            Filter = "*.lua"
        };
        _debounce = new Debounce(500);
        _fileSystemWatcher.Changed += OnChanged;
        _fileSystemWatcher.EnableRaisingEvents = true;
    }

    private async void OnChanged(object source, FileSystemEventArgs e)
    {
        try
        {
            await _debounce.InvokeAsync(() =>
            {
                OnReload?.Invoke();
            });
        }
        catch (Exception)
        {
            // Ignore
        }
    }
}