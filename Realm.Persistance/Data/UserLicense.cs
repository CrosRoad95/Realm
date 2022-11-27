namespace Realm.Persistance.Data;

public class UserLicense
{
#pragma warning disable CS8618
    public Guid UserId { get; set; }
    public string LicenseId { get; set; }
    public DateTime? SuspendedUntil { get; set; }
    public string? SuspendedReason { get; set; }
#pragma warning restore CS8618

    public User? User { get; set; }

    public bool IsSuspended() => SuspendedUntil != null && SuspendedUntil > DateTime.Now;
}
