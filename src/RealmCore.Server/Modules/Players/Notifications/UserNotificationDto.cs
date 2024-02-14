namespace RealmCore.Server.Modules.Players.Notifications;

public sealed class UserNotificationDto : IEquatable<UserNotificationDto>
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public DateTime SentTime { get; set; }
    public DateTime? ReadTime { get; set; }
    public string Title { get; set; }
    public string Excerpt { get; set; }
    public string Content { get; set; }

    [return: NotNullIfNotNull(nameof(userNotificationData))]
    public static UserNotificationDto? Map(UserNotificationData? userNotificationData)
    {
        if (userNotificationData == null)
            return null;

        return new UserNotificationDto
        {
            Id = userNotificationData.Id,
            UserId = userNotificationData.UserId,
            SentTime = userNotificationData.SentTime,
            ReadTime = userNotificationData.ReadTime,
            Title = userNotificationData.Title,
            Excerpt = userNotificationData.Excerpt,
            Content = userNotificationData.Content,
        };
    }

    public bool Equals(UserNotificationDto? other)
    {
        return other?.Id == Id;
    }
}
