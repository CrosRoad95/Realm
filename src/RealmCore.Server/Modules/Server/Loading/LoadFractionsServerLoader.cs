namespace RealmCore.Server.Modules.Server.Loading;

internal sealed class LoadFractionsServerLoader : IServerLoader
{
    private readonly IFractionsService _fractionService;

    public LoadFractionsServerLoader(IFractionsService fractionService)
    {
        _fractionService = fractionService;
    }

    public async Task Load()
    {
        await _fractionService.LoadFractions();
    }
}
