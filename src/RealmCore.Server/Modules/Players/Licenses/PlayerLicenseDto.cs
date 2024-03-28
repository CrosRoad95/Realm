namespace RealmCore.Server.Modules.Players.Licenses;

public sealed class PlayerLicenseDto
{
    public required int LicenseId { get; init; }
    public required DateTime? SuspendedUntil { get; init; }
    public required string? SuspendedReason { get; init; }

    public bool IsSuspended(DateTime now) => SuspendedUntil != null && SuspendedUntil > now;

    [return: NotNullIfNotNull(nameof(userLicenseData))]
    public static PlayerLicenseDto? Map(UserLicenseData? userLicenseData)
    {
        if (userLicenseData == null)
            return null;

        return new PlayerLicenseDto
        {
            LicenseId = userLicenseData.LicenseId,
            SuspendedReason = userLicenseData.SuspendedReason,
            SuspendedUntil = userLicenseData.SuspendedUntil
        };
    }
}
