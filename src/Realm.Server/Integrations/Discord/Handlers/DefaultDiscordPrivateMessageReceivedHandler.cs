using Realm.Module.Discord.Interfaces;
using SlipeServer.Server.Services;

namespace Realm.Server.Integrations.Discord.Handlers;

public class DefaultDiscordPrivateMessageReceivedHandler : IDiscordPrivateMessageReceived
{
    private readonly ChatBox _chatBox;

    public DefaultDiscordPrivateMessageReceivedHandler(ChatBox chatBox)
    {
        _chatBox = chatBox;
    }

    public void HandlePrivateMessage(ulong userId, ulong messageId, string content)
    {
        _chatBox.Output($"{userId}: {content}");
    }
}
