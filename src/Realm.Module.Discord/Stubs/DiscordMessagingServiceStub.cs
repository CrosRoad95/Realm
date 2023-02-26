namespace Realm.Module.Discord.Stubs;

public class DiscordMessagingServiceStub : Messaging.MessagingBase
{
    public DiscordMessagingServiceStub()
    {

    }

    public override Task<SendMessageResponse> SendMessage(SendMessageRequest request, ServerCallContext context)
    {
        return base.SendMessage(request, context);
    }
}
