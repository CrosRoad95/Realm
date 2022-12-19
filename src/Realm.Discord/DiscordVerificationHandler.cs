using SlipeServer.Server.ElementCollections;

namespace Realm.Module.Discord;

internal class DiscordVerificationHandler
{
    private readonly IElementCollection _elementCollection;

    public DiscordVerificationHandler(IElementCollection elementCollection)
    {
        _elementCollection = elementCollection;
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
