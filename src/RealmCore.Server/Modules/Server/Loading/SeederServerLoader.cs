namespace RealmCore.Server.Modules.Server.Loading;

internal sealed class SeederServerLoader : IServerLoader
{
    private readonly SeederServerBuilder _seederServerBuilder;

    public SeederServerLoader(SeederServerBuilder seederServerBuilder)
    {
        _seederServerBuilder = seederServerBuilder;
    }

    public async Task Load()
    {
        await _seederServerBuilder.Build();
    }
}
