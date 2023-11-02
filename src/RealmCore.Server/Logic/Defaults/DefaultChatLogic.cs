using SlipeServer.Server.Extensions;

namespace RealmCore.Server.Logic.Defaults;

public class DefaultChatLogic
{
    private readonly ChatBox _chatBox;
    private readonly ILogger<DefaultChatLogic> _logger;
    private readonly IElementCollection _elementCollection;

    public DefaultChatLogic(MtaServer server, ChatBox chatBox, ILogger<DefaultChatLogic> logger, IElementCollection elementCollection)
    {
        _chatBox = chatBox;
        _logger = logger;
        _elementCollection = elementCollection;
        server.PlayerJoined += (player) =>
        {
            player.CommandEntered += HandlePlayerCommandEntered;
        };
    }

    private void HandlePlayerCommandEntered(Player plr, PlayerCommandEventArgs arguments)
    {
        switch (arguments.Command)
        {
            case "say":
                var player = (RealmPlayer)plr;
                if (player.IsLoggedIn)
                {
                    var messageContent = string.Join(' ', arguments.Arguments);
                    string message = $"{player.Name}: {messageContent}";
                    string messageWithColors = $"{player.NametagColor.ToColorCode()}{player.Name}: #ffffff{messageContent}";
                    foreach (var targetPlayer in _elementCollection.GetByType<RealmPlayer>())
                    {
                        if(targetPlayer.IsLoggedIn)
                            _chatBox.OutputTo(targetPlayer, messageWithColors, Color.White, true);
                    }

                    _logger.LogInformation("{message}", message);
                }
                else
                {
                    _chatBox.OutputTo(player, "Nie możesz pisać ponieważ nie jesteś zalogowany.");
                }
                break;
        }
    }
}
