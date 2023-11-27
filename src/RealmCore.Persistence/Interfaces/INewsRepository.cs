namespace RealmCore.Persistence.Interfaces;

public interface INewsRepository
{
    Task Add(string title, string excerpt, string content, DateTime publishTime, string[] tags, int? createdByUserId = null, CancellationToken cancellationToken = default);
    Task<List<NewsData>> Get(DateTime now, int limit = 10, CancellationToken cancellationToken = default);
}
