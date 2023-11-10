namespace RealmCore.Server.DTOs;

public class UserEventDTO
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public int EventType { get; set; }
    public string? Metadata { get; set; }
    public DateTime DateTime { get; set; }

    [return: NotNullIfNotNull(nameof(userEventData))]
    public static UserEventDTO? Map(UserEventData? userEventData)
    {
        if (userEventData == null)
            return null;

        return new UserEventDTO
        {
            DateTime = userEventData.DateTime,
            UserId = userEventData.UserId,
            EventType = userEventData.EventType,
            Metadata = userEventData.Metadata,
            Id = userEventData.Id
        };
    }
}
