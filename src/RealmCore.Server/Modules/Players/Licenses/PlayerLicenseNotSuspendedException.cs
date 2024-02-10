namespace RealmCore.Server.Modules.Players.Licenses;

public class PlayerLicenseNotSuspendedException : PlayerLicenseException
{
    public PlayerLicenseNotSuspendedException(int licenseId) : base(licenseId)
    {
    }
}
