namespace RealmCore.Persistence.Repository;

public interface IUploadedFilesRepository
{
    Task<UploadFileData> Add(string name, string fileType, ulong size, string metadata, DateTime uploadedAt, int? userId = null, CancellationToken cancellationToken = default);
    Task<UploadFileData[]> GetById(int id, CancellationToken cancellationToken = default);
    Task<UploadFileData?> GetByName(string name, CancellationToken cancellationToken = default);
    Task<UploadFileData[]> GetByUserId(int userId, CancellationToken cancellationToken = default);
}

internal sealed class UploadedFilesRepository : IUploadedFilesRepository
{
    private readonly IDb _db;

    public UploadedFilesRepository(IDb db)
    {
        _db = db;
    }

    public async Task<UploadFileData[]> GetById(int id, CancellationToken cancellationToken = default)
    {
        using var activity = Activity.StartActivity(nameof(GetByName));
        if (activity != null)
        {
            activity.AddTag("Id", id);
        }

        var query = _db.UploadFiles.Where(x => x.Id == id);

        return await query.ToArrayAsync(cancellationToken);
    }
    
    public async Task<UploadFileData[]> GetByUserId(int userId, CancellationToken cancellationToken = default)
    {
        using var activity = Activity.StartActivity(nameof(GetByName));
        if (activity != null)
        {
            activity.AddTag("UserId", userId);
        }

        var query = _db.UserUploadFiles.Where(x => x.UserId == userId)
            .Include(x => x.UploadFile)
            .Select(x => x.UploadFile);

        return await query.ToArrayAsync(cancellationToken);
    }
    
    public async Task<UploadFileData?> GetByName(string name, CancellationToken cancellationToken = default)
    {
        using var activity = Activity.StartActivity(nameof(GetByName));
        if (activity != null)
        {
            activity.AddTag("Name", name);
        }

        var query = _db.UploadFiles.Where(x => x.Name == name);

        return await query.FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<UploadFileData> Add(string name, string fileType, ulong size, string metadata, DateTime uploadedAt, int? userId = null, CancellationToken cancellationToken = default)
    {
        using var activity = Activity.StartActivity(nameof(Add));
        if (activity != null)
        {
            activity.AddTag("Name", name);
            activity.AddTag("FileType", fileType);
            activity.AddTag("Size", size);
        }

        if(userId != null)
        {
            var userUploadFileData = new UserUploadFileData
            {
                UserId = userId.Value,
                UploadFile = new UploadFileData
                {
                    Name = name,
                    FileType = fileType,
                    Size = size,
                    Metadata = metadata,
                    UploadedAt = uploadedAt
                }
            };
            _db.UserUploadFiles.Add(userUploadFileData);
            await _db.SaveChangesAsync(cancellationToken);
            return userUploadFileData.UploadFile;
        }
        else
        {
            var uploadFile = new UploadFileData
            {
                Name = name,
                FileType = fileType,
                Size = size,
                Metadata = metadata,
                UploadedAt = uploadedAt
            };
            _db.UploadFiles.Add(uploadFile);
            await _db.SaveChangesAsync(cancellationToken);
            return uploadFile;
        }
    }

    public static readonly ActivitySource Activity = new("RealmCore.UploadedFilesRepository", "1.0.0");
}