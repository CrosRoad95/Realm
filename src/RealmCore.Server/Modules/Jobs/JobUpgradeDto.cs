namespace RealmCore.Server.Modules.Jobs;

public class JobUpgradeDto
{
    public short JobId { get; set; }
    public int UpgradeId { get; set; }

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

}
