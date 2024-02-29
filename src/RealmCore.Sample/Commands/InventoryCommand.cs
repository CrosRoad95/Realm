namespace RealmCore.Sample.Commands;

[CommandName("inventory")]
public sealed class InventoryCommand : IInGameCommand
{
    private readonly ILogger<InventoryCommand> _logger;
    private readonly ChatBox _chatBox;

    public string[] RequiredPolicies { get; } = [];
    public InventoryCommand(ILogger<InventoryCommand> logger, ChatBox chatBox)
    {
        _logger = logger;
        _chatBox = chatBox;
    }

    public Task Handle(RealmPlayer player, CommandArguments args, CancellationToken cancellationToken)
    {
        if(player.Inventory.TryGetPrimary(out var inventory))
        {
            _chatBox.OutputTo(player, $"Inventory, {inventory.Number}/{inventory.Size}");
            foreach (var item in inventory.Items)
            {
                _chatBox.OutputTo(player, $"Item, {item.ItemId} = {item.Name}");
            }
        }

        return Task.CompletedTask;
    }
}
