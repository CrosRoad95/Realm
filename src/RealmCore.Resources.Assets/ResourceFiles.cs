﻿using SlipeServer.Resources.Base;
using System.Reflection;

namespace RealmCore.Resources.Assets;

internal class ResourceFiles
{
    private static Assembly Assembly { get; } = Assembly.GetExecutingAssembly();
    public static byte[] Assets { get; } = EmbeddedResourceHelper.GetLuaFile("RealmCore.Resources.Assets.Lua.assets.lua", Assembly);
}
