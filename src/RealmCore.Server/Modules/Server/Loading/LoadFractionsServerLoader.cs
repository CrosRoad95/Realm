namespace RealmCore.Server.Modules.Server.Loading;

internal sealed class LoadFractionsServerLoader : IServerLoader
{
    private readonly IFractionService _fractionService;

    public LoadFractionsServerLoader(IFractionService fractionService)
    {
        _fractionService = fractionService;
    }

    public async Task Load()
    {
        await _fractionService.LoadFractions();
    }
}
