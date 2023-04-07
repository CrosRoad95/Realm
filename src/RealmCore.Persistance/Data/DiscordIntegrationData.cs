namespace RealmCore.Persistance.Data;

public sealed class DiscordIntegrationData
{
    public int UserId { get; set; }
    public ulong DiscordUserId { get; set; }

    public UserData? User { get; set; }
}
