namespace Realm.Persistance.Data;

public sealed class DiscordIntegration
{
    public int UserId { get; set; }
    public ulong DiscordUserId { get; set; }

    public User? User { get; set; }
}
