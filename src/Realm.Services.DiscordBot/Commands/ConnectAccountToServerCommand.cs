namespace Realm.DiscordBot.Commands;

internal class ConnectAccountToServerCommand : InteractionModuleBase<SocketInteractionContext>
{
    private readonly DiscordBotConfiguration _discordConfiguration;

    public ConnectAccountToServerCommand(DiscordBotConfiguration discordConfiguration)
    {
        _discordConfiguration = discordConfiguration;
    }

    [SlashCommand("polaczkonto", "Łączenie konta discord z serwerem mta", false, RunMode.Async)]
    public async Task ConnectAccount(string kod)
    {
        if(_discordConfiguration.ConnectServerAccountChannel == null)
        {
            await RespondAsync("Ten serwer nie wspiera łączenia kont discord z serwerem mta lub konfiguracja serwera jest niepoprawna.", ephemeral: true);
            return;
        }

        if(Context.Channel.Id != _discordConfiguration.ConnectServerAccountChannel.ChannelId)
        {
            await RespondAsync("Nie możesz użyć tej komendy na tym kanale.", ephemeral: true);
            return;
        }

        if (!Guid.TryParse(kod, out var _))
        {
            await RespondAsync("Kod jest niepoprawny", ephemeral: true);
            return;
        }

        //var response = await _discordVerificationHandler.VerifyCodeWithResponse(kod, Context.User.Id);
        //if (response != null)
        //{
        //    await RespondAsync(response, ephemeral: true);
        //}
        //else
        //{
        //    await RespondAsync("Podany kod jest nieaktualny lub niepoprawny.", ephemeral: true);
        //}
    }
}
