namespace RealmCore.Server.Modules.Vehicles.Tuning;

public class UpgradeAlreadyExistsException : Exception
{
    public UpgradeAlreadyExistsException(short jobId, int upgradeId) : base($"Upgrade '{upgradeId}' already exists for job id '{jobId}'") { }
}
