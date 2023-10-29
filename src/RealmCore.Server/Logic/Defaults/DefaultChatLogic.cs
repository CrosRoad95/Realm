using SlipeServer.Server.Extensions;

namespace RealmCore.Server.Logic.Defaults;

public class DefaultChatLogic
{
    private readonly ChatBox _chatBox;
    private readonly ILogger<DefaultChatLogic> _logger;
    private readonly IEntityEngine _entityEngine;

    public DefaultChatLogic(MtaServer server, ChatBox chatBox, ILogger<DefaultChatLogic> logger, IEntityEngine entityEngine)
    {
        _chatBox = chatBox;
        _logger = logger;
        _entityEngine = entityEngine;

        server.PlayerJoined += (player) =>
        {
            player.CommandEntered += HandlePlayerCommandEntered;
        };
    }

    private void HandlePlayerCommandEntered(Player player, PlayerCommandEventArgs arguments)
    {
        switch (arguments.Command)
        {
            case "say":
                if (_entityEngine.TryGetEntityByPlayer(player, out var playerEntity))
                    if (playerEntity.HasComponent<UserComponent>())
                    {
                        string message = $"{player.NametagColor.ToColorCode()}{player.Name}: #ffffff{string.Join(' ', arguments.Arguments)}";
                        foreach (var targetPlayerEntity in _entityEngine.PlayerEntities.Where(x => x.HasComponent<UserComponent>()))
                            _chatBox.OutputTo(targetPlayerEntity, message, Color.White, true);

                        _logger.LogInformation("{message}", message);
                    }
                    else
                    {
                        _chatBox.OutputTo(playerEntity, "Nie możesz pisać ponieważ nie jesteś zalogowany.");
                    }
                break;
        }
    }
}
