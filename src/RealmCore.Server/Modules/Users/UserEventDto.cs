namespace RealmCore.Server.Modules.Users;

public class UserEventDto
{
    public required int Id { get; init; }
    public required int UserId { get; init; }
    public required int EventType { get; init; }
    public required string? Metadata { get; init; }
    public required DateTime DateTime { get; init; }

    [return: NotNullIfNotNull(nameof(userEventData))]
    public static UserEventDto? Map(UserEventData? userEventData)
    {
        if (userEventData == null)
            return null;

        return new UserEventDto
        {
            Id = userEventData.Id,
            UserId = userEventData.UserId,
            EventType = userEventData.EventType,
            Metadata = userEventData.Metadata,
            DateTime = userEventData.DateTime
        };
    }
}
