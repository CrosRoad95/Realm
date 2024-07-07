namespace RealmCore.Persistence.Data;

public sealed class DiscordIntegrationData
{
    public int UserId { get; set; }
    public ulong DiscordUserId { get; set; }
    public string? Name { get; set; }

    public UserData? User { get; set; }
}
