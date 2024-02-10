namespace RealmCore.Server.Modules.Players.Jobs;

public sealed class JobUpgradeDto : IEquatable<JobUpgradeDto>
{
    public short JobId { get; init; }
    public int UpgradeId { get; init; }

    [return: NotNullIfNotNull(nameof(jobUpgradeData))]
    public static JobUpgradeDto? Map(JobUpgradeData? jobUpgradeData)
    {
        if (jobUpgradeData == null)
            return null;

        return new JobUpgradeDto
        {
            JobId = jobUpgradeData.JobId,
            UpgradeId = jobUpgradeData.UpgradeId
        };
    }

    public bool Equals(JobUpgradeDto? other)
    {
        if (other == null)
            return false;

        return other.JobId == JobId && other.UpgradeId == UpgradeId;
    }
}
