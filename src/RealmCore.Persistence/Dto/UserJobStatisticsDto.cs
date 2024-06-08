using System.Diagnostics.CodeAnalysis;

namespace RealmCore.Persistence.Dto;

public sealed class UserJobStatisticsDto
{
    public required int UserId { get; init; }
    public required int JobId { get; init; }
    public required ulong Points { get; init; }
    public required ulong TimePlayed { get; init; }

    [return: NotNullIfNotNull(nameof(userJobStatisticsData))]
    public static UserJobStatisticsDto? Map(JobStatisticsData? userJobStatisticsData)
    {
        if (userJobStatisticsData == null)
            return null;

        return new UserJobStatisticsDto
        {
            JobId = userJobStatisticsData.JobId,
            Points = userJobStatisticsData.Points,
            TimePlayed = userJobStatisticsData.TimePlayed,
            UserId = userJobStatisticsData.UserId
        };
    }

}
