using SlipeServer.Server.ElementCollections;
using SlipeServer.Server.Elements;
using Realm.Module.Scripting.Extensions;
using Realm.Module.Discord.Scripting.Events;

namespace Realm.Module.Discord;

internal class DiscordVerificationHandler
{
    private readonly IElementCollection _elementCollection;
    private readonly EventScriptingFunctions _eventFunctions;

    public DiscordVerificationHandler(IElementCollection elementCollection, EventScriptingFunctions eventFunctions)
    {
        _elementCollection = elementCollection;
        _eventFunctions = eventFunctions;
    }

    public async Task<string?> VerifyCodeWithResponse(string code, ulong discordAccountId)
    {
        //foreach (var player in _elementCollection.GetByType<Player>())
        //{
        //    if (player.IsLoggedIn && player.Account != null)
        //    {
        //        if (player.Account.IsDiscordConnectionCodeValid(code))
        //        {
        //            try
        //            {
        //                await player.Account.SetDiscordUserId(discordAccountId);
        //                if (player.Account.Discord == null)
        //                    return null;

        //                using var @event = new DiscordPlayerConnectedEvent(player, player.Account.Discord);
        //                await _eventFunctions.InvokeEvent(@event);
        //                return "Pomyślnie połączyłeś konto discord z serwerem";
        //            }
        //            catch (Exception)
        //            {
        //                return null;
        //            }
        //        }
        //    }
        //}
        return null;
    }
}
