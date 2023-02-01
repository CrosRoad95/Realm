using SlipeServer.Server.Extensions;
using System.Drawing;

namespace Realm.Server.Logic;

internal class ChatLogic
{
    private readonly MtaServer _server;
    private readonly ChatBox _chatBox;
    private readonly ILogger _logger;
    private readonly IEntityByElement _entityByElement;
    private readonly ECS _ecs;

    public ChatLogic(MtaServer server, ChatBox chatBox, ILogger logger, IEntityByElement entityByElement, ECS ecs)
    {
        _server = server;
        _chatBox = chatBox;
        _logger = logger;
        _entityByElement = entityByElement;
        _ecs = ecs;

        server.PlayerJoined += (player) =>
        {
            player.CommandEntered += HandlePlayerCommandEntered;
        };
    }

    private void HandlePlayerCommandEntered(Player player, SlipeServer.Server.Elements.Events.PlayerCommandEventArgs arguments)
    {
        switch (arguments.Command)
        {
            case "say":
                var playerEntity = _entityByElement.TryGetEntityByPlayer(player);
                if (playerEntity != null && playerEntity.TryGetComponent(out PlayerElementComponent playerElementComponent))
                {
                    if (playerEntity.HasComponent<AccountComponent>())
                    {
                        string message = $"{player.NametagColor.ToColorCode()}{player.Name}: #ffffff{string.Join(' ', arguments.Arguments)}";
                        foreach (var targetPlayerEntity in _ecs.GetPlayerEntities().Where(x => x.HasComponent<AccountComponent>()))
                            targetPlayerEntity.GetRequiredComponent<PlayerElementComponent>().SendChatMessage(message, Color.White, true);

                        _logger.Information("{message}", message);
                    }
                    else
                    {
                        playerElementComponent.SendChatMessage("Nie możesz pisać ponieważ nie jesteś zalogowany.");
                    }
                }
                break;
        }
    }
}
