﻿namespace RealmCore.Persistence.Data;

public sealed class UserLicenseData
{
#pragma warning disable CS8618
    public int UserId { get; set; }
    public int LicenseId { get; set; }
    public DateTime? SuspendedUntil { get; set; }
    public string? SuspendedReason { get; set; }

    public bool IsSuspended(DateTime now) => SuspendedUntil != null && SuspendedUntil > now;
#pragma warning restore CS8618
}
