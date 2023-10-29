namespace RealmCore.Server.Commands;

[CommandName("serverinfo")]
internal class ServerInfoCommand : ICommand
{
    private readonly ILogger<HelpCommand> _logger;
    private readonly IEntityEngine _entityEngine;
    private readonly MtaServer _mtaServer;
    private readonly IDateTimeProvider _dateTimeProvider;
    private readonly IElementCollection _elementCollection;
    private readonly IMapsService _mapsService;

    public ServerInfoCommand(ILogger<HelpCommand> logger, IEntityEngine entityEngine, MtaServer mtaServer, IDateTimeProvider dateTimeProvider, IElementCollection elementCollection, IMapsService mapsService)
    {
        _logger = logger;
        _entityEngine = entityEngine;
        _mtaServer = mtaServer;
        _dateTimeProvider = dateTimeProvider;
        _elementCollection = elementCollection;
        _mapsService = mapsService;
    }

    public Task Handle(Entity entity, CommandArguments args)
    {
        _logger.LogInformation("Server uptime: {uptime}", _dateTimeProvider.Now - _mtaServer.StartDatetime);
        _logger.LogInformation("Players: {playerCount}, logged in players: {loggedInPlayers}", _elementCollection.GetByType<Player>().Count(), _entityEngine.PlayerEntities.Where(x => x.HasComponent<UserComponent>()).Count());
        _logger.LogInformation("Vehicles: {vehiclesCount}", _entityEngine.VehicleEntities.Count());
        _logger.LogInformation("Entities count: {entitiesCount}, total components count: {entitiesComponentsCount}", _entityEngine.EntitiesCount, _entityEngine.EntitiesComponentsCount);
        _logger.LogInformation("Loaded maps: {loadedMaps}", _mapsService.Maps);

        return Task.CompletedTask;
    }
}
