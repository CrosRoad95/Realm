namespace RealmCore.Server.Modules.Commands.Administration;

[CommandName("serverinfo")]
internal class ServerInfoCommand : IInGameCommand
{
    public string[] RequiredPolicies { get; } = ["Owner"];

    private readonly ILogger<HelpCommand> _logger;
    private readonly MtaServer _server;
    private readonly IDateTimeProvider _dateTimeProvider;
    private readonly IElementCollection _elementCollection;
    private readonly MapsService _mapsService;

    public ServerInfoCommand(ILogger<HelpCommand> logger, MtaServer server, IDateTimeProvider dateTimeProvider, IElementCollection elementCollection, MapsService mapsService)
    {
        _logger = logger;
        _server = server;
        _dateTimeProvider = dateTimeProvider;
        _elementCollection = elementCollection;
        _mapsService = mapsService;
    }

    public Task Handle(RealmPlayer player, CommandArguments args, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Server uptime: {uptime}", _dateTimeProvider.Now - _server.StartDatetime);
        _logger.LogInformation("Players: {playerCount}, logged in players: {loggedInPlayers}", _elementCollection.GetByType<RealmPlayer>().Count(), _elementCollection.GetByType<RealmPlayer>().Where(x => x.User.IsLoggedIn).Count());
        _logger.LogInformation("Vehicles: {vehiclesCount}", _elementCollection.GetByType<RealmVehicle>());
        _logger.LogInformation("Elements count: {elementsCount}", _elementCollection.Count);
        _logger.LogInformation("Loaded global maps: {loadedMaps}", string.Join(", ", _mapsService.LoadedMaps));

        return Task.CompletedTask;
    }
}
