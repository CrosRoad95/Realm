namespace Realm.Server.Commands;

internal class SaveCommand : ICommand
{
    private readonly IInternalRPGServer _rpgServer;

    public string CommandName => "save";

    public SaveCommand(IInternalRPGServer rpgServer)
    {
        _rpgServer = rpgServer;
    }

    public async Task HandleCommand(string command)
    {
        await _rpgServer.ECS.SaveAll();
    }
}
