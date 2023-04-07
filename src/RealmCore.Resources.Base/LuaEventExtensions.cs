using SlipeServer.Packets.Definitions.Lua;
using SlipeServer.Server.Events;
using SlipeServer.Server.Mappers;

namespace RealmCore.Resources.Base;

public static class LuaEventExtensions
{
    private static void VerifyParametersCount(ref LuaEvent luaEvent, int expectedCount)
    {
        if (luaEvent.Parameters.Length != expectedCount)
            throw new ArgumentException($"Parameters count doesn't match, expected {expectedCount} parameter/s, got: {luaEvent.Parameters.Length}");
    }

    private static T Map<T>(LuaValue luaValue, FromLuaValueMapper fromLuaValueMapper)
    {
        if (typeof(T) == typeof(LuaValue))
            return (T)(object)luaValue;
        return (T)fromLuaValueMapper.Map(typeof(T), luaValue);
    }

    public static T Read<T>(this LuaEvent luaEvent, FromLuaValueMapper fromLuaValueMapper)
    {
        VerifyParametersCount(ref luaEvent, 1);
        return Map<T>(luaEvent.Parameters[0], fromLuaValueMapper);
    }

    public static (T1, T2) Read<T1, T2>(this LuaEvent luaEvent, FromLuaValueMapper fromLuaValueMapper)
    {
        VerifyParametersCount(ref luaEvent, 2);

        return (
            Map<T1>(luaEvent.Parameters[0], fromLuaValueMapper),
            Map<T2>(luaEvent.Parameters[1], fromLuaValueMapper)
        );
    }

    public static (T1, T2, T3) Read<T1, T2, T3>(this LuaEvent luaEvent, FromLuaValueMapper fromLuaValueMapper)
    {
        VerifyParametersCount(ref luaEvent, 3);

        return (
            Map<T1>(luaEvent.Parameters[0], fromLuaValueMapper),
            Map<T2>(luaEvent.Parameters[1], fromLuaValueMapper),
            Map<T3>(luaEvent.Parameters[2], fromLuaValueMapper)
        );
    }

    public static (T1, T2, T3, T4) Read<T1, T2, T3, T4>(this LuaEvent luaEvent, FromLuaValueMapper fromLuaValueMapper)
    {
        VerifyParametersCount(ref luaEvent, 4);

        return (
            Map<T1>(luaEvent.Parameters[0], fromLuaValueMapper),
            Map<T2>(luaEvent.Parameters[1], fromLuaValueMapper),
            Map<T3>(luaEvent.Parameters[2], fromLuaValueMapper),
            Map<T4>(luaEvent.Parameters[3], fromLuaValueMapper)
        );
    }

    public static (T1, T2, T3, T4, T5) Read<T1, T2, T3, T4, T5>(this LuaEvent luaEvent, FromLuaValueMapper fromLuaValueMapper)
    {
        VerifyParametersCount(ref luaEvent, 5);

        return (
            Map<T1>(luaEvent.Parameters[0], fromLuaValueMapper),
            Map<T2>(luaEvent.Parameters[1], fromLuaValueMapper),
            Map<T3>(luaEvent.Parameters[2], fromLuaValueMapper),
            Map<T4>(luaEvent.Parameters[3], fromLuaValueMapper),
            Map<T5>(luaEvent.Parameters[4], fromLuaValueMapper)
        );
    }

    public static (T1, T2, T3, T4, T5, T6) Read<T1, T2, T3, T4, T5, T6>(this LuaEvent luaEvent, FromLuaValueMapper fromLuaValueMapper)
    {
        VerifyParametersCount(ref luaEvent, 6);

        return (
            Map<T1>(luaEvent.Parameters[0], fromLuaValueMapper),
            Map<T2>(luaEvent.Parameters[1], fromLuaValueMapper),
            Map<T3>(luaEvent.Parameters[2], fromLuaValueMapper),
            Map<T4>(luaEvent.Parameters[3], fromLuaValueMapper),
            Map<T5>(luaEvent.Parameters[4], fromLuaValueMapper),
            Map<T6>(luaEvent.Parameters[5], fromLuaValueMapper)
        );
    }

    public static (T1, T2, T3, T4, T5, T6, T7) Read<T1, T2, T3, T4, T5, T6, T7>(this LuaEvent luaEvent, FromLuaValueMapper fromLuaValueMapper)
    {
        VerifyParametersCount(ref luaEvent, 7);

        return (
            Map<T1>(luaEvent.Parameters[0], fromLuaValueMapper),
            Map<T2>(luaEvent.Parameters[1], fromLuaValueMapper),
            Map<T3>(luaEvent.Parameters[2], fromLuaValueMapper),
            Map<T4>(luaEvent.Parameters[3], fromLuaValueMapper),
            Map<T5>(luaEvent.Parameters[4], fromLuaValueMapper),
            Map<T6>(luaEvent.Parameters[5], fromLuaValueMapper),
            Map<T7>(luaEvent.Parameters[6], fromLuaValueMapper)
        );
    }

    public static (T1, T2, T3, T4, T5, T6, T7, T8) Read<T1, T2, T3, T4, T5, T6, T7, T8>(this LuaEvent luaEvent, FromLuaValueMapper fromLuaValueMapper)
    {
        VerifyParametersCount(ref luaEvent, 8);

        return (
            Map<T1>(luaEvent.Parameters[0], fromLuaValueMapper),
            Map<T2>(luaEvent.Parameters[1], fromLuaValueMapper),
            Map<T3>(luaEvent.Parameters[2], fromLuaValueMapper),
            Map<T4>(luaEvent.Parameters[3], fromLuaValueMapper),
            Map<T5>(luaEvent.Parameters[4], fromLuaValueMapper),
            Map<T6>(luaEvent.Parameters[5], fromLuaValueMapper),
            Map<T7>(luaEvent.Parameters[6], fromLuaValueMapper),
            Map<T8>(luaEvent.Parameters[7], fromLuaValueMapper)
        );
    }

}
