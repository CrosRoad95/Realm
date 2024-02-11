namespace RealmCore.Server.Modules.Users;

public class UserEventDto
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public int EventType { get; set; }
    public string? Metadata { get; set; }
    public DateTime DateTime { get; set; }

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
