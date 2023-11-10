namespace RealmCore.Server.DTOs;

public class JobUpgradeDTO
{
    public short JobId { get; set; }
    public int UpgradeId { get; set; }

    [return: NotNullIfNotNull(nameof(jobUpgradeData))]
    public static JobUpgradeDTO? Map(JobUpgradeData? jobUpgradeData)
    {
        if (jobUpgradeData == null)
            return null;

        return new JobUpgradeDTO
        {
            JobId = jobUpgradeData.JobId,
            UpgradeId = jobUpgradeData.UpgradeId
        };
    }

}
