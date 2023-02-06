namespace Realm.Domain.Components.Players;

[ComponentUsage(false)]
public class DiscordComponent : Component
{
    public const string ClaimDiscordUserIdName = "discord.user.id";

    private string? _discordConnectionCode = null;
    private DateTime? _discordConnectionCodeValidUntil = null;
    public DiscordUser? Discord { get; private set; }

    public DiscordComponent()
    {
    }

    public bool IsConnectedWithDiscordAccount()
    {
        return Discord != null;
    }

    public bool IsDiscordConnectionCodeValid(string code)
    {
        if (!HasPendingDiscordConnectionCode())
            return false;

        return _discordConnectionCode == code;
    }

    public bool HasPendingDiscordConnectionCode()
    {
        return _discordConnectionCodeValidUntil != null || _discordConnectionCodeValidUntil > DateTime.Now;
    }

    public void InvalidateDiscordConnectionCode()
    {
        _discordConnectionCode = null;
        _discordConnectionCodeValidUntil = null;
    }

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
