namespace RealmCore.Server.Commands;

[CommandName("reloadelements")]
internal class ReloadElementsCommand : ICommand
{
    private readonly IElementCollection _elementCollection;
    private readonly ISaveService _saveService;
    private readonly ILogger<SaveCommand> _logger;
    private readonly ILoadService _loadService;

    public ReloadElementsCommand(IElementCollection elementCollection, ISaveService saveService, ILogger<SaveCommand> logger, ILoadService loadService)
    {
        _elementCollection = elementCollection;
        _saveService = saveService;
        _logger = logger;
        _loadService = loadService;
    }

    public async Task Handle(RealmPlayer player, CommandArguments args)
    {
        int savedElements = 0;
        foreach (var element in _elementCollection.GetAll())
        {
            try
            {
                if (await _saveService.BeginSave(element))
                {
#if DEBUG
                    await _saveService.Commit();
#endif
                    savedElements++;
                    element.Destroy();
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to save element: {elementName}", element.ToString());
            }
        }
#if !DEBUG
        await _saveService.Commit();
#endif
        await _loadService.LoadAll();
    }
}
