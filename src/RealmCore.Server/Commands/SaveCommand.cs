namespace RealmCore.Server.Commands;

[CommandName("save")]
internal class SaveCommand : ICommand
{
    private readonly IElementCollection _elementCollection;
    private readonly ISaveService _saveService;
    private readonly ILogger<SaveCommand> _logger;

    public SaveCommand(IElementCollection elementCollection, ISaveService saveService, ILogger<SaveCommand> logger)
    {
        _elementCollection = elementCollection;
        _saveService = saveService;
        _logger = logger;
    }

    public async Task Handle(RealmPlayer player, CommandArguments args)
    {
        int savedElements = 0;
        foreach (var element in _elementCollection.GetByType<Player>())
        {
            using var _ = _logger.BeginElement(element);
            try
            {
#if DEBUG
                if (await _saveService.BeginSave(element))
                {
                    await _saveService.Commit();
                    savedElements++;
                }
#else
                if (await _saveService.BeginSave(element))
                    savedElements++;
#endif
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to save element: {elementName}", element.ToString());
            }
        }
#if !DEBUG
        await _saveService.Commit();
#endif
    }
}
