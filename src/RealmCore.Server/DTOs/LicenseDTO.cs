namespace RealmCore.Server.DTOs;

public class LicenseDTO
{
    public int LicenseId { get; set; }
    public DateTime? SuspendedUntil { get; set; }
    public string? SuspendedReason { get; set; }

    public bool IsSuspended(DateTime now) => SuspendedUntil != null && SuspendedUntil > now;
}
