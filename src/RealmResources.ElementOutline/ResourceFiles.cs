﻿using SlipeServer.Resources.Base;
using System.Reflection;

namespace Realm.Resources.ElementOutline;

internal class ResourceFiles
{
    private static Assembly Assembly { get; } = Assembly.GetExecutingAssembly();
    public static byte[] Outline { get; } = EmbeddedResourceHelper.GetLuaFile("Realm.Resources.ElementOutline.Lua.outline.lua", Assembly);
}