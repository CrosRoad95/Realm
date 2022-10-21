namespace Realm.Identity.Data;

public sealed class User : IdentityUser<Guid> {
    public ulong? DiscordId { get; set; }
}
