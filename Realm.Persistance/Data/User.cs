namespace Realm.Persistance.Data;

public sealed class User : IdentityUser<Guid>
{
    public ulong? DiscordId { get; set; }
}
