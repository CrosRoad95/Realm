using Realm.Persistance.Scripting.Classes;

namespace Realm.Domain.New;

public class DiscordComponent : Component
{
    public const string ClaimDiscordUserIdName = "discord.user.id";

    private string? _discordConnectionCode = null;
    private DateTime? _discordConnectionCodeValidUntil = null;
    public DiscordUser? Discord { get; private set; }

    public DiscordComponent()
    {
    }

    [ScriptMember("isConnectedWithDiscordAccount")]
    public bool IsConnectedWithDiscordAccount()
    {
        return Discord != null;
    }

    [ScriptMember("isDiscordConnectionCodeValid")]
    public bool IsDiscordConnectionCodeValid(string code)
    {
        if (!HasPendingDiscordConnectionCode())
            return false;

        return _discordConnectionCode == code;
    }

    [ScriptMember("hasPendingDiscordConnectionCode")]
    public bool HasPendingDiscordConnectionCode()
    {
        return _discordConnectionCodeValidUntil != null || _discordConnectionCodeValidUntil > DateTime.Now;
    }

    [ScriptMember("invalidateDiscordConnectionCode")]
    public void InvalidateDiscordConnectionCode()
    {
        _discordConnectionCode = null;
        _discordConnectionCodeValidUntil = null;
    }

    [ScriptMember("generateAndGetDiscordConnectionCode")]
    public string? GenerateAndGetDiscordConnectionCode(int validForMinutes = 2)
    {
        if (IsConnectedWithDiscordAccount())
            return null;

        if (validForMinutes <= 0)
            return null;
        _discordConnectionCode = Guid.NewGuid().ToString();
        _discordConnectionCodeValidUntil = DateTime.Now.AddMinutes(validForMinutes);
        return _discordConnectionCode;
    }
}
