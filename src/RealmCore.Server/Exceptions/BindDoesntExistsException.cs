﻿namespace RealmCore.Server.Exceptions;

public class BindDoesntExistsException : Exception
{
    public BindDoesntExistsException(string key) : base($"Bind with key '{key}' doesn't exists.")
    {
    }
}