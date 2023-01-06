namespace Realm.Domain.Exceptions;

public class UpgradeAlreadyExistsException : Exception
{
    public UpgradeAlreadyExistsException(short jobId, string upgradeName) : base($"Upgrade '{upgradeName}' already exists for job id '{jobId}'") { }
}
