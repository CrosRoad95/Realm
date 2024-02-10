namespace RealmCore.Server.Modules.Players.Licenses;

public class PlayerLicenseAlreadySuspendedException : PlayerLicenseException
{
    public PlayerLicenseAlreadySuspendedException(int licenseId) : base(licenseId)
    {
    }
}
