namespace RealmCore.Server.Modules.Players.Licenses;

public class LicenseDto
{
    public int LicenseId { get; set; }
    public DateTime? SuspendedUntil { get; set; }
    public string? SuspendedReason { get; set; }

    public bool IsSuspended(DateTime now) => SuspendedUntil != null && SuspendedUntil > now;


    [return: NotNullIfNotNull(nameof(userLicenseData))]
    public static LicenseDto? Map(UserLicenseData? userLicenseData)
    {
        if (userLicenseData == null)
            return null;

        return new LicenseDto
        {
            LicenseId = userLicenseData.LicenseId,
            SuspendedReason = userLicenseData.SuspendedReason,
            SuspendedUntil = userLicenseData.SuspendedUntil
        };
    }
}
