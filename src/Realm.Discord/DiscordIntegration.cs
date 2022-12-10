using Realm.Interfaces.Grpc;
using Realm.Module.Discord.Scripting.Events;
using Realm.Module.Scripting.Extensions;
using Realm.Module.Scripting.Functions;
using Realm.Module.Scripting.Interfaces;

namespace Realm.Module.Discord;

internal class DiscordIntegration
{
    private readonly EventScriptingFunctions _eventScriptingFunctions;

    public DiscordIntegration(EventScriptingFunctions eventScriptingFunctions, IGrpcDiscord grpcDiscord)
    {
        _eventScriptingFunctions = eventScriptingFunctions;
        grpcDiscord.UpdateStatusChannel = HandleUpdateStatusChannel;
    }

    public async Task<string> HandleUpdateStatusChannel()
    {
        using var @event = new DiscordStatusChannelUpdateContext();
        await _eventScriptingFunctions.InvokeEvent(DiscordStatusChannelUpdateContext.EventName, @event);
        return @event.Content;
    }

    public void InitializeScripting(IScriptingModuleInterface scriptingModuleInterface)
    {
        // Events
        _eventScriptingFunctions.RegisterEvent(DiscordPlayerConnectedEvent.EventName);
        _eventScriptingFunctions.RegisterEvent(DiscordUserChangedEvent.EventName);
        _eventScriptingFunctions.RegisterEvent(DiscordStatusChannelUpdateContext.EventName);

        // Javascript types
        scriptingModuleInterface.AddHostType(typeof(DiscordPlayerConnectedEvent));
        scriptingModuleInterface.AddHostType(typeof(DiscordUserChangedEvent));
        scriptingModuleInterface.AddHostType(typeof(DiscordStatusChannelUpdateContext));
    }
}