namespace RealmCore.Server.Structs;

public struct License
{
    public int licenseId;
    public DateTime? suspendedUntil;
    public string? suspendedReason;

    public bool IsSuspended(IDateTimeProvider dateTimeProvider) => suspendedUntil != null && suspendedUntil > dateTimeProvider.Now;
}