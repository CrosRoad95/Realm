namespace RealmCore.Persistence.Repository;

public sealed class SerialDiscordUserIdRepository
{
    private readonly IDb _db;

    public SerialDiscordUserIdRepository(IDb db)
    {
        _db = db;
    }

    public async Task<bool> Add(string serial, ulong discordId, CancellationToken cancellationToken = default)
    {
        using var activity = Activity.StartActivity(nameof(Add));

        if (activity != null)
        {
            activity.AddTag("Serial", serial);
            activity.AddTag("DiscordId", discordId);
        }

        try
        {
            _db.SerialDiscordUserId.Add(new SerialDiscordUserIdData
            {
                DiscordId = discordId,
                Serial = serial
            });
            await _db.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateException)
        {
            return false;
        }
        finally
        {
            _db.ChangeTracker.Clear();
        }

        return true;
    }

    public static readonly ActivitySource Activity = new("RealmCore.BusinessesRepository", "1.0.0");
}
