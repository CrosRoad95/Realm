namespace Realm.Persistance.Entities;

public class Test : IId
{
    public Guid Id { get; set; }
    public int Number { get; set; }
    public string Text { get; set; }
}
