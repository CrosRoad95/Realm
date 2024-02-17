namespace RealmCore.Server.Modules.Commands.Administration;

[CommandName("serverinfo")]
internal class ServerInfoCommand : ICommand
{
    private readonly ILogger<HelpCommand> _logger;
    private readonly MtaServer _mtaServer;
    private readonly IDateTimeProvider _dateTimeProvider;
    private readonly IElementCollection _elementCollection;
    private readonly IMapsService _mapsService;

    public ServerInfoCommand(ILogger<HelpCommand> logger, MtaServer mtaServer, IDateTimeProvider dateTimeProvider, IElementCollection elementCollection, IMapsService mapsService)
    {
        _logger = logger;
        _mtaServer = mtaServer;
        _dateTimeProvider = dateTimeProvider;
        _elementCollection = elementCollection;
        _mapsService = mapsService;
    }

    public Task Handle(RealmPlayer player, CommandArguments args)
    {
        _logger.LogInformation("Server uptime: {uptime}", _dateTimeProvider.Now - _mtaServer.StartDatetime);
        _logger.LogInformation("Players: {playerCount}, logged in players: {loggedInPlayers}", _elementCollection.GetByType<RealmPlayer>().Count(), _elementCollection.GetByType<RealmPlayer>().Where(x => x.User.IsSignedIn).Count());
        _logger.LogInformation("Vehicles: {vehiclesCount}", _elementCollection.GetByType<RealmVehicle>());
        _logger.LogInformation("Elements count: {elementsCount}", _elementCollection.Count);
        _logger.LogInformation("Loaded global maps: {loadedMaps}", string.Join(", ", _mapsService.LoadedMaps));

        return Task.CompletedTask;
    }
}
