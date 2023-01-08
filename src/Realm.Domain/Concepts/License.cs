namespace Realm.Domain.Concepts;

public struct License
{
    public string licenseId;
    public DateTime? suspendedUntil;
    public string? suspendedReason;
    public bool IsSuspended => suspendedUntil != null && suspendedUntil > DateTime.Now;
}