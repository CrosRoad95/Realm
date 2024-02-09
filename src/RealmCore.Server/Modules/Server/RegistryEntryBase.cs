namespace RealmCore.Server.Modules.Server;

public abstract class RegistryEntryBase<TKey>
{
    public TKey Id { get; internal set; }
}

public abstract class RegistryEntryBase : RegistryEntryBase<int>
{

}
