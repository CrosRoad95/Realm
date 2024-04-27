using RealmCore.Module.Discord.Interfaces;

namespace RealmCore.Console.Extra.Integrations.Discord.Handlers;

public class DiscordPrivateMessageReceivedHandler : IDiscordPrivateMessageReceivedHandler
{
    private readonly ChatBox _chatBox;

    public DiscordPrivateMessageReceivedHandler(ChatBox chatBox)
    {
        _chatBox = chatBox;
    }

    public Task HandlePrivateMessage(ulong userId, ulong messageId, string content, CancellationToken cancellationToken)
    {
        _chatBox.Output($"{userId}: {content}");
        return Task.CompletedTask;
    }
}
