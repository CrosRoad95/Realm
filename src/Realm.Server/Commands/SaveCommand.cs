namespace Realm.Server.Commands;

internal class SaveCommand : ICommand
{
    private readonly ILoadAndSaveService _loadAndSaveService;

    public string CommandName => "save";

    public SaveCommand(ILoadAndSaveService loadAndSaveService)
    {
        _loadAndSaveService = loadAndSaveService;
    }

    public async Task HandleCommand(string command)
    {
        await _loadAndSaveService.SaveAll();
    }
}
