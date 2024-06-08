using RealmCore.Server.Modules.Players.Groups;

namespace RealmCore.BlazorGui.Commands;

[CommandName("creategroup")]
public sealed class CreateGroupCommand : IInGameCommand
{
    private readonly ILogger<CreateGroupCommand> _logger;
    private readonly IGroupsService _groupService;
    private readonly ChatBox _chatBox;

    public string[] RequiredPolicies { get; } = [];
    public CreateGroupCommand(ILogger<CreateGroupCommand> logger, IGroupsService groupService, ChatBox chatBox)
    {
        _logger = logger;
        _groupService = groupService;
        _chatBox = chatBox;
    }

    public async Task Handle(RealmPlayer player, CommandArguments args, CancellationToken cancellationToken)
    {
        var name = args.ReadWordOrDefault("default");
        try
        {
            var group = await _groupService.CreateGroup(name, "", cancellationToken: cancellationToken);
            await _groupService.TryAddMember(player, group.id, 100, "Leader", cancellationToken);

            _chatBox.OutputTo(player, $"Group: '{name}' has been created");
        }
        catch (Exception)
        {
            _chatBox.OutputTo(player, $"Failed to create group: '{name}'");
            throw;
        }
    }
}
