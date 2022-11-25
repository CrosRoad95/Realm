using Realm.Discord;
using Realm.Scripting.Functions;

namespace Realm.Server.Services;

internal class DiscordVerificationHandler : IDiscordVerificationHandler
{
    private readonly IElementCollection _elementCollection;
    private readonly DiscordConfiguration _discordConfiguration;
    private readonly EventScriptingFunctions _eventFunctions;

    public DiscordVerificationHandler(IElementCollection elementCollection, DiscordConfiguration discordConfiguration, EventScriptingFunctions eventFunctions)
    {
        _elementCollection = elementCollection;
        _discordConfiguration = discordConfiguration;
        _eventFunctions = eventFunctions;
    }

    public async Task<string?> VerifyCodeWithResponse(string code, ulong discordAccountId)
    {
        if (_discordConfiguration.ConnectServerAccountChannel == null)
            return null;

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

                        using var discord = new PlayerDiscordConnectedEvent(rpgPlayer, rpgPlayer.Account.Discord);
                        await _eventFunctions.InvokeEvent(discord);
                        return _discordConfiguration.ConnectServerAccountChannel.SuccessMessage;
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
