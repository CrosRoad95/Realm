namespace RealmCore.Server.DTOs;

public class LicenseDTO
{
    public int LicenseId { get; set; }
    public DateTime? SuspendedUntil { get; set; }
    public string? SuspendedReason { get; set; }

    public bool IsSuspended(DateTime now) => SuspendedUntil != null && SuspendedUntil > now;


    [return: NotNullIfNotNull(nameof(userLicenseData))]
    public static LicenseDTO? Map(UserLicenseData? userLicenseData)
    {
        if (userLicenseData == null)
            return null;

        return new LicenseDTO
        {
            LicenseId = userLicenseData.LicenseId,
            SuspendedReason = userLicenseData.SuspendedReason,
            SuspendedUntil = userLicenseData.SuspendedUntil
        };
    }

}
