namespace RealmCore.Server.Modules.Commands.Administration;

[CommandName("reloadelements")]
internal class ReloadElementsCommand : IInGameCommand
{
    public string[] RequiredPolicies { get; } = ["Owner"];

    private readonly IElementCollection _elementCollection;
    private readonly ElementSaveService _saveService;
    private readonly ILogger<SaveCommand> _logger;
    private readonly VehicleLoader _vehicleLoader;

    public ReloadElementsCommand(IElementCollection elementCollection, ElementSaveService saveService, ILogger<SaveCommand> logger, VehicleLoader vehicleLoader)
    {
        _elementCollection = elementCollection;
        _saveService = saveService;
        _logger = logger;
        _vehicleLoader = vehicleLoader;
    }

    public async Task Handle(RealmPlayer player, CommandArguments args, CancellationToken cancellationToken)
    {
        int savedElements = 0;
        foreach (var element in _elementCollection.GetAll())
        {
            try
            {
                if (await _saveService.Save(cancellationToken))
                {
                    savedElements++;
                    if(element is not RealmPlayer)
                        element.Destroy();
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to save element: {elementName}", element.ToString());
            }
        }

        await _vehicleLoader.LoadAll(cancellationToken);
    }
}
