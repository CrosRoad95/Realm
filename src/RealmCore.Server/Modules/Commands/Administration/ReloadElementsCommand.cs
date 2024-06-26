﻿namespace RealmCore.Server.Modules.Commands.Administration;

[CommandName("reloadelements")]
internal class ReloadElementsCommand : IInGameCommand
{
    public string[] RequiredPolicies { get; } = ["Owner"];

    private readonly IElementCollection _elementCollection;
    private readonly IElementSaveService _saveService;
    private readonly ILogger<SaveCommand> _logger;
    private readonly IVehicleLoader _loadService;

    public ReloadElementsCommand(IElementCollection elementCollection, IElementSaveService saveService, ILogger<SaveCommand> logger, IVehicleLoader loadService)
    {
        _elementCollection = elementCollection;
        _saveService = saveService;
        _logger = logger;
        _loadService = loadService;
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

        await _loadService.LoadAll(cancellationToken);
    }
}
