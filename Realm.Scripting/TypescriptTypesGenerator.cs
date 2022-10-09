using Realm.Common.Attributes;
using Realm.Common.Extensions;
using System.Reflection;

namespace Realm.Scripting;

public class TypescriptTypesGenerator
{
    private readonly List<Type> _types = new List<Type>();
    public TypescriptTypesGenerator()
    {
        AddType(typeof(object));
    }

    public TypescriptTypesGenerator AddType(Type type)
    {
        _types.Add(type);
        return this;
    }

    private string ResolveTypeName(Type type)
    {
        var isNullable = type.IsNullable();
        if (isNullable)
            type = type.GetGenericArguments()[0];

        string name = "NotImplemented";
        if (type == typeof(string))
            name = "string";
        else if (type.IsNumericType())
            name = "number";
        else if (type == typeof(bool))
            name = "boolean";
        else if (type == typeof(void))
            name = "void";
        else if (type == typeof(ISpawn))
            name = "Spawn";
        else if (type == typeof(Vector2))
            name = "Vector2";
        else if (type == typeof(Vector3))
            name = "Vector3";
        else if (type == typeof(Vector4))
            name = "Vector4";
        else if (type == typeof(Matrix4x4))
            name = "Matrix4x4";
        else if (type == typeof(Quaternion))
            name = "Quaternion";
        else if (type == typeof(Type))
            name = "Type";
        else if (type == typeof(object))
            name = "Object";
        else if (type.IsArray)
            name = ResolveTypeName(type.GetElementType()) + "[]";

        return name;
    }

    private string ResolveTypescriptPropertyName(Type type)
    {
        var isNullable = type.IsNullable();
        var name = ResolveTypeName(type);
        if (name == string.Empty)
            throw new NotImplementedException($"Not implemented type: {type.Name}");

        return $"{name}{(isNullable ? " | null" : string.Empty)}";
    }

    private string ResolvePropertyInfoName(PropertyInfo propertyInfo)
    {
        var isNullable = propertyInfo.IsNullable();

        return $"{propertyInfo.Name.ToTypescriptName()}{(isNullable? "?" : "")}";
    }

    private string ResolveParameterInfoName(ParameterInfo parameterInfo, ref bool wasNullable)
    {
        var isNullable = wasNullable || parameterInfo.IsNullable();
        if (isNullable)
            wasNullable = true;
        var name = parameterInfo.Name.ToTypescriptName();
        var typeName = ResolveTypescriptPropertyName(parameterInfo.ParameterType);
        return $"{name}{(isNullable ? "?" : "")}: {typeName}";
    }

    private string BuildTypescriptTypeForType(Type type)
    {
        var sb = new StringBuilder();
        var extends = type == typeof(object) ? "" : " extends Object";
        var nameAttribute = type.GetCustomAttribute<NameAttribute>();
        var className = type.Name;
        if (nameAttribute != null)
            className = nameAttribute.Name;

        sb.AppendLine($"export interface {className}{extends} {{");

        foreach (var propertyInfo in type.GetPublicProperties())
        {
            var name = ResolvePropertyInfoName(propertyInfo);
            var typescriptType = ResolveTypescriptPropertyName(propertyInfo.PropertyType);
            sb.AppendLine($"  {name}: {typescriptType};");
        }

        foreach (var methodInfo in type.GetMethods()
            .Where(x => (type.IsInterface || x.DeclaringType == type) && !x.IsSpecialName))
        {
            bool wasNullable = false;
            var methodName = methodInfo.Name.ToTypescriptName();
            var parameters = string.Join(", ", methodInfo.GetParameters().Select(x => ResolveParameterInfoName(x, ref wasNullable)));
            var returnType = ResolveTypescriptPropertyName(methodInfo.ReturnType);
            sb.AppendLine($"  {methodName}({parameters}): {returnType};");
        }

        sb.AppendLine("}");

        return sb.ToString();
    }

    public string Build()
    {
        var types = _types.Select(BuildTypescriptTypeForType)
            .Prepend("export interface NotImplemented { }");
        return string.Join("\r\n\r\n", types);
    }
}
