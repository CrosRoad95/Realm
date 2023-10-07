namespace RealmCore.Console.Commands;

[CommandName("creategroup")]
public sealed class CreateGroupCommand : IInGameCommand
{
    private readonly ILogger<CreateGroupCommand> _logger;
    private readonly IGroupService _groupService;
    private readonly ChatBox _chatBox;

    public CreateGroupCommand(ILogger<CreateGroupCommand> logger, IGroupService groupService, ChatBox chatBox)
    {
        _logger = logger;
        _groupService = groupService;
        _chatBox = chatBox;
    }

    public async Task Handle(Entity entity, CommandArguments args)
    {
        var name = args.ReadWordOrDefault("default");
        try
        {
            var group = await _groupService.CreateGroup(name, "");
            await _groupService.AddMember(entity, group.id, 100, "Leader");

            _chatBox.OutputTo(entity, $"Group: '{name}' has been created");
        }
        catch (Exception)
        {
            _chatBox.OutputTo(entity, $"Failed to create group: '{name}'");
            throw;
        }
    }
}
