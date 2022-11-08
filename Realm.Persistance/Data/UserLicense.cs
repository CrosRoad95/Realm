namespace Realm.Persistance.Data;

public class UserLicense
{
    public Guid UserId { get; set; }
    public string LicenseId { get; set; }
    public DateTime? SuspendedUntil { get; set; }
    public string? SuspendedReason { get; set; }

    public User? User { get; set; }

    public bool IsSuspended() => SuspendedUntil != null && SuspendedUntil > DateTime.Now;
}
