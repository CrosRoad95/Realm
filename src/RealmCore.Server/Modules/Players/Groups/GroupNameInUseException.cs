namespace RealmCore.Server.Modules.Players.Groups;

public class GroupNameInUseException : Exception
{
    public string GroupName { get; }

    public GroupNameInUseException(string groupName) : base($"Group '{groupName}' is already in use")
    {
        GroupName = groupName;
    }
}
