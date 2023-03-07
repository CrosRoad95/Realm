using Realm.Module.Discord.Interfaces;

namespace Realm.Server.Integrations.Discord.Handlers;

public class DefaultDiscordPrivateMessageReceivedHandler : IDiscordPrivateMessageReceived
{
    private readonly ChatBox _chatBox;

    public DefaultDiscordPrivateMessageReceivedHandler(ChatBox chatBox)
    {
        _chatBox = chatBox;
    }

    public Task HandlePrivateMessage(ulong userId, ulong messageId, string content)
    {
        _chatBox.Output($"{userId}: {content}");
        return Task.CompletedTask;
    }
}
