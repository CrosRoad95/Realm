namespace Realm.Scripting.ClearscriptExtensions;

internal class LowercaseSymbolsLoader : CustomAttributeLoader
{
    public override T[] LoadCustomAttributes<T>(ICustomAttributeProvider resource, bool inherit)
    {
        var declaredAttributes = base.LoadCustomAttributes<T>(resource, inherit);
        if (IsProtectedResource(resource))
        {
            return declaredAttributes;
        }
        if (!declaredAttributes.Any() && typeof(T) == typeof(ScriptMemberAttribute) && resource is MemberInfo member)
        {
            return new[] { new ScriptMemberAttribute(member.Name.ToTypescriptName()) } as T[];
        }
        return declaredAttributes;
    }

    private static bool IsProtectedResource(ICustomAttributeProvider resource)
    {
        if (resource is MemberInfo member && member.DeclaringType != null)
        {
            if (member.DeclaringType.Assembly.GetName().Name == "ClearScript.Core")
            {
                return true;
            }
            var typeName = member.DeclaringType.FullName;
            return
                typeName.StartsWith("System.Collections.Generic.IAsyncEnumerator") ||
                typeName.StartsWith("System.Collections.Generic.IEnumerator") ||
                typeName == "System.Collections.IEnumerator" ||
                typeName == "System.IDisposable";
        }
        return false;
    }
}