namespace RealmCore.Server.Modules.Players.Notifications;

public sealed class UserNotificationDto : IEquatable<UserNotificationDto>
{
    public required int Id { get; set; }
    public required int UserId { get; set; }
    public required int Type { get; set; }
    public required DateTime SentTime { get; set; }
    public required DateTime? ReadTime { get; set; }
    public required string Title { get; set; }
    public required string Excerpt { get; set; }
    public required string Content { get; set; }

    public bool IsRead => ReadTime != null;

    [return: NotNullIfNotNull(nameof(data))]
    public static UserNotificationDto? Map(UserNotificationData? data)
    {
        if (data == null)
            return null;

        return new UserNotificationDto
        {
            Id = data.Id,
            Type = data.Type,
            UserId = data.UserId,
            SentTime = data.SentTime,
            ReadTime = data.ReadTime,
            Title = data.Title,
            Excerpt = data.Excerpt,
            Content = data.Content,
        };
    }

    public bool Equals(UserNotificationDto? other)
    {
        return other?.Id == Id;
    }
}
