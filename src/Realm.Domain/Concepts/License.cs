namespace Realm.Domain.Concepts;

public struct License
{
    public int licenseId;
    public DateTime? suspendedUntil;
    public string? suspendedReason;
    public bool IsSuspended => suspendedUntil != null && suspendedUntil > DateTime.Now;
}