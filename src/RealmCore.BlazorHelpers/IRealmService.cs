namespace RealmCore.BlazorHelpers;

public interface IRealmService<T>
{
    T Service { get; }
}
