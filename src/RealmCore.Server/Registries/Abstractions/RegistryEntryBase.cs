namespace RealmCore.Server.Registries.Abstractions;

public abstract class RegistryEntryBase<TKey>
{
    public TKey Id { get; internal set; }
}

public abstract class RegistryEntryBase : RegistryEntryBase<int>
{

}
