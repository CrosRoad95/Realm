﻿namespace Realm.Server.Extensions;

internal static class EventFunctionsExtensions
{
    public static async Task InvokeEvent<TEvent>(this EventFunctions eventFunctions, TEvent @event) where TEvent : INamedLuaEvent
    {
        await eventFunctions.InvokeEvent(TEvent.EventName, @event);
    }
}