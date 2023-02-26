namespace Realm.Console.Commands;

[CommandName("creategroup")]
public sealed class CreateGroupCommand : IIngameCommand
{
    private readonly ILogger<CreateGroupCommand> _logger;
    private readonly IGroupService _groupService;

    public CreateGroupCommand(ILogger<CreateGroupCommand> logger, IGroupService groupService)
    {
        _logger = logger;
        _groupService = groupService;
    }

    public async Task Handle(Guid traceId, Entity entity, string[] args)
    {
        var name = args.FirstOrDefault("default");
        try
        {
            var group = await _groupService.CreateGroup(name, "");
            await _groupService.AddMember(group.id, entity, 100, "Leader");

            entity.GetRequiredComponent<PlayerElementComponent>().SendChatMessage($"Group: '{name}' has been created");
        }
        catch (Exception)
        {
            entity.GetRequiredComponent<PlayerElementComponent>().SendChatMessage($"Failed to create group: '{name}'");
            throw;
        }
    }
}
