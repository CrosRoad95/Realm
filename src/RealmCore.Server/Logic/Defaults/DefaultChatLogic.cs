﻿using SlipeServer.Server.Extensions;

namespace RealmCore.Server.Logic.Defaults;

public class DefaultChatLogic
{
    private readonly ChatBox _chatBox;
    private readonly ILogger<DefaultChatLogic> _logger;
    private readonly IECS _ecs;

    public DefaultChatLogic(MtaServer server, ChatBox chatBox, ILogger<DefaultChatLogic> logger, IECS ecs)
    {
        _chatBox = chatBox;
        _logger = logger;
        _ecs = ecs;

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
                if (_ecs.TryGetEntityByPlayer(player, out var playerEntity))
                    if (playerEntity.TryGetComponent(out PlayerElementComponent playerElementComponent))
                    {
                        if (playerEntity.HasComponent<UserComponent>())
                        {
                            string message = $"{player.NametagColor.ToColorCode()}{player.Name}: #ffffff{string.Join(' ', arguments.Arguments)}";
                            foreach (var targetPlayerEntity in _ecs.PlayerEntities.Where(x => x.HasComponent<UserComponent>()))
                                targetPlayerEntity.GetRequiredComponent<PlayerElementComponent>().SendChatMessage(message, Color.White, true);

                            _logger.LogInformation("{message}", message);
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