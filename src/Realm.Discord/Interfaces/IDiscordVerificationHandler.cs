namespace Realm.Module.Discord.Interfaces;

public interface IDiscordVerificationHandler
{
    Task<bool> HandleVerifyCode(string code, ulong discordAccountId);
}
