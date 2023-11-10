namespace RealmCore.Server.DTOs;

public class UserStatDTO
{
    public int StatId { get; set; }
    public float Value { get; set; }

    [return: NotNullIfNotNull(nameof(userStatData))]
    public static UserStatDTO? Map(UserStatData? userStatData)
    {
        if (userStatData == null)
            return null;

        return new UserStatDTO
        {
            StatId = userStatData.StatId,
            Value = userStatData.Value,
        };
    }

}
