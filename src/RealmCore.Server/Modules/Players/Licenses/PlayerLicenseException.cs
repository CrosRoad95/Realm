namespace RealmCore.Server.Modules.Players.Licenses;

public class PlayerLicenseException : Exception
{
    public int LicenseId { get; }

    public PlayerLicenseException(int licenseId)
    {
        LicenseId = licenseId;
    }
}
