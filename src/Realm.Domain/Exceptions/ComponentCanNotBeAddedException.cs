﻿namespace Realm.Domain.Exceptions;

public class ComponentCanNotBeAddedException<T> : Exception
{
    public ComponentCanNotBeAddedException() : base($"Only one instance of component '{typeof(T).Name}' can be added to one entity") { }
}