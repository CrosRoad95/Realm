namespace RealmCore.Server.Interfaces;

public interface INewsService
{
    Task<List<NewsDTO>> Get(int limit = 10);
}
