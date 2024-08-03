namespace RealmCore.Server.Modules.Chat;

public class DefaultChatHostedService : PlayerLifecycle, IHostedService
{
    private readonly ChatBox _chatBox;
    private readonly ILogger<DefaultChatHostedService> _logger;
    private readonly IElementCollection _elementCollection;

    public DefaultChatHostedService(PlayersEventManager playersEventManager, ChatBox chatBox, ILogger<DefaultChatHostedService> logger, IElementCollection elementCollection) : base(playersEventManager)
    {
        _chatBox = chatBox;
        _logger = logger;
        _elementCollection = elementCollection;
    }

    public Task StartAsync(CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }

    protected override void PlayerJoined(RealmPlayer player)
    {
        player.CommandEntered += HandlePlayerCommandEntered;
    }

    protected override void PlayerLeft(RealmPlayer player)
    {
        player.CommandEntered -= HandlePlayerCommandEntered;
    }

    private void HandlePlayerCommandEntered(Player plr, PlayerCommandEventArgs arguments)
    {
        switch (arguments.Command)
        {
            case "say":
                var player = (RealmPlayer)plr;
                if (player.User.IsLoggedIn)
                {
                    var messageContent = string.Join(' ', arguments.Arguments);
                    string message = $"{player.Name}: {messageContent}";
                    string messageWithColors = $"{player.NametagColor.ToColorCode()}{player.Name}: #ffffff{messageContent}";
                    foreach (var targetPlayer in _elementCollection.GetByType<RealmPlayer>())
                    {
                        if (targetPlayer.User.IsLoggedIn)
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
