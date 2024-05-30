namespace RealmCore.Server.Modules.Players.Notifications;

public sealed class UserNotificationDto : IEquatable<UserNotificationDto>
{
    public required int Id { get; set; }
    public required int UserId { get; set; }
    public required DateTime SentTime { get; set; }
    public required DateTime? ReadTime { get; set; }
    public required string Title { get; set; }
    public required string Excerpt { get; set; }
    public required string Content { get; set; }

    public bool IsRead => ReadTime != null;

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
