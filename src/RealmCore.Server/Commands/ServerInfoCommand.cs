namespace RealmCore.Server.Commands;

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
        _logger.LogInformation("Players: {playerCount}, logged in players: {loggedInPlayers}", _elementCollection.GetByType<Player>().Count(), _elementCollection.GetByType<Player>().Cast<RealmPlayer>().Where(x => x.HasComponent<UserComponent>()));
        _logger.LogInformation("Vehicles: {vehiclesCount}", _elementCollection.GetByType<Vehicle>());
        _logger.LogInformation("Elements count: {elementsCount}", _elementCollection.Count);
        _logger.LogInformation("Loaded maps: {loadedMaps}", _mapsService.Maps);

        return Task.CompletedTask;
    }
}
