namespace Realm.Scripting.Classes.Events;

public class FormSubmitDataEvent
{
    public IRPGPlayer Player { get; set; }
    public string Name { get; set; }
    public Dictionary<string, string> Data { get; set; }
    public List<string> Errors { get; } = new();

    public void AddError(string error)
    {
        Errors.Add(error);
    }

    public override string ToString() => "FormSubmitDataEvent";
}
