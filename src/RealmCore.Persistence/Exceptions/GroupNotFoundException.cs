﻿namespace RealmCore.Persistence.Exceptions;

public class GroupNotFoundException : Exception
{
    public GroupNotFoundException(string groupName) : base($"Group of name '{groupName}' not found")
    {

    }
}
