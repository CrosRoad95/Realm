namespace Realm.Persistance.Data;

public class DiscordIntegration
{
    public int UserId { get; set; }
    public ulong DiscordUserId { get; set; }

    public virtual User? User { get; set; }
}
