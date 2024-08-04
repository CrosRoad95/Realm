namespace RealmCore.Server.Modules.Commands.Administration;

[CommandName("save")]
internal class SaveCommand : IInGameCommand
{
    public string[] RequiredPolicies { get; } = ["Owner"];

    private readonly IElementCollection _elementCollection;
    private readonly ElementSaveService _saveService;
    private readonly ILogger<SaveCommand> _logger;

    public SaveCommand(IElementCollection elementCollection, ElementSaveService saveService, ILogger<SaveCommand> logger)
    {
        _elementCollection = elementCollection;
        _saveService = saveService;
        _logger = logger;
    }

    public async Task Handle(RealmPlayer player, CommandArguments args, CancellationToken cancellationToken)
    {
        int savedElements = 0;
        foreach (var element in _elementCollection.GetByType<RealmPlayer>())
        {
            using var _ = _logger.BeginElement(element);
            try
            {
                if (await _saveService.Save(cancellationToken))
                {
                    savedElements++;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to save element: {elementName}", element.ToString());
            }
        }
    }
}
