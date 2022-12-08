using Realm.Interfaces.Grpc;
using Realm.Server.Scripting.Events;

namespace Realm.Discord;

internal class DiscordIntegration
{
    private readonly EventScriptingFunctions _eventScriptingFunctions;

    public DiscordIntegration(EventScriptingFunctions eventScriptingFunctions, IGrpcDiscord grpcDiscord)
    {
        _eventScriptingFunctions = eventScriptingFunctions;
        grpcDiscord.UpdateStatusChannel = UpdateStatusChannel;
    }

    public async Task<string> UpdateStatusChannel()
    {
        return "test...";
    }

    public void InitializeScripting(IScriptingModuleInterface scriptingModuleInterface)
    {
        // Events
        _eventScriptingFunctions.RegisterEvent(DiscordPlayerConnectedEvent.EventName);
        _eventScriptingFunctions.RegisterEvent(DiscordUserChangedEvent.EventName);

        // Javascript types
        scriptingModuleInterface.AddHostType(typeof(DiscordPlayerConnectedEvent));
        scriptingModuleInterface.AddHostType(typeof(DiscordUserChangedEvent));
        scriptingModuleInterface.AddHostType(typeof(DiscordStatusChannelUpdateContext));
    }
}