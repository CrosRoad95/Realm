namespace RealmCore.Server.Modules.Server;

public abstract class CollectionItemBase<TKey>
{
    public TKey Id { get; internal set; }
}

public abstract class CollectionItemBase : CollectionItemBase<int>
{

}
