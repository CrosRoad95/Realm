namespace Realm.Discord.Interfaces;

public interface IDiscordVerificationHandler
{
    Task<string?> VerifyCodeWithResponse(string code, ulong userId);
}
