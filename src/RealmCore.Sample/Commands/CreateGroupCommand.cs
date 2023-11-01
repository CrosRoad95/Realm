namespace RealmCore.Sample.Commands;

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

    public async Task Handle(RealmPlayer player, CommandArguments args)
    {
        var name = args.ReadWordOrDefault("default");
        try
        {
            var group = await _groupService.CreateGroup(name, "");
            await _groupService.AddMember(player, group.id, 100, "Leader");

            _chatBox.OutputTo(player, $"Group: '{name}' has been created");
        }
        catch (Exception)
        {
            _chatBox.OutputTo(player, $"Failed to create group: '{name}'");
            throw;
        }
    }
}
