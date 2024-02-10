namespace RealmCore.Server.Modules.Players.Licenses;

public class PlayerLicenseNotFoundException : PlayerLicenseException
{
    public PlayerLicenseNotFoundException(int licenseId) : base(licenseId)
    {
    }
}
