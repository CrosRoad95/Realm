﻿namespace RealmCore.Persistence.Repository;

internal sealed class UserWhitelistedSerialsRepository : IUserWhitelistedSerialsRepository
{
    private readonly IDb _db;

    public UserWhitelistedSerialsRepository(IDb db)
    {
        _db = db;
    }

    public async Task<bool> IsSerialWhitelisted(int userId, string serial, CancellationToken cancellationToken = default)
    {
        var query = _db.UserWhitelistedSerials
            .AsNoTracking()
            .TagWith(nameof(UserWhitelistedSerialsRepository))
            .Where(x => x.UserId == userId && x.Serial == serial);
        return await query.AnyAsync(cancellationToken).ConfigureAwait(false);
    }

    public async Task<bool> TryAddWhitelistedSerial(int userId, string serial, CancellationToken cancellationToken = default)
    {
        if (serial.Length != 32)
            throw new ArgumentException(null, nameof(serial));

        try
        {
            _db.UserWhitelistedSerials.Add(new UserWhitelistedSerialData
            {
                Serial = serial,
                UserId = userId
            });

            var added = await _db.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
            return added > 0;
        }
        catch (Exception)
        {
            return false;
        }
        finally
        {
            _db.ChangeTracker.Clear();
        }
    }

    public async Task<bool> TryRemoveWhitelistedSerial(int userId, string serial, CancellationToken cancellationToken = default)
    {
        if (serial.Length != 32)
            throw new ArgumentException(null, nameof(serial));

        try
        {
            var deleted = await _db.UserWhitelistedSerials
                .AsNoTracking()
                .TagWith(nameof(UserWhitelistedSerialsRepository))
                .Where(x => x.UserId == userId && x.Serial == serial)
                .ExecuteDeleteAsync(cancellationToken).ConfigureAwait(false);

            return deleted > 0;
        }
        catch (Exception)
        {
            return false;
        }
        finally
        {
            _db.ChangeTracker.Clear();
        }
    }
}