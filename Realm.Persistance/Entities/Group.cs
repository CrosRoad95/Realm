namespace Realm.Persistance.Entities;

public class Group : IId
{
    public Guid Id { get; set; }
    public string Name { get; set; }

    public List<UserAccount> Users { get; set; } = new();
}
