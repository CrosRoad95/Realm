using Realm.Domain.Elements;
using Realm.Server.Scripting.Events;
using SlipeServer.Server.ElementCollections;
using SlipeServer.Server.Elements;
using Realm.Scripting.Extensions;

namespace Realm.Server.Services;

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
        foreach (var rpgPlayer in _elementCollection.GetByType<Player>().Cast<RPGPlayer>())
        {
            if(rpgPlayer.IsLoggedIn && rpgPlayer.Account != null)
            {
                if(rpgPlayer.Account.IsDiscordConnectionCodeValid(code))
                {
                    try
                    {
                        await rpgPlayer.Account.SetDiscordUserId(discordAccountId);
                        if (rpgPlayer.Account.Discord == null)
                            return null;

                        using var @event = new DiscordPlayerConnectedEvent(rpgPlayer, rpgPlayer.Account.Discord);
                        await _eventFunctions.InvokeEvent(@event);
                        return "Pomyślnie połączyłeś konto discord z serwerem";
                    }
                    catch(Exception)
                    {
                        return null;
                    }
                }
            }
        }
        return null;
    }
}
