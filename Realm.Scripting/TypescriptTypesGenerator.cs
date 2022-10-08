using System.Collections.ObjectModel;

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

    public bool IsNullable(PropertyInfo property) =>
        IsNullableHelper(property.PropertyType, property.DeclaringType, property.CustomAttributes);

    public bool IsNullable(FieldInfo field) =>
        IsNullableHelper(field.FieldType, field.DeclaringType, field.CustomAttributes);

    public bool IsNullable(ParameterInfo parameter) =>
        IsNullableHelper(parameter.ParameterType, parameter.Member, parameter.CustomAttributes);
    
    public bool IsNullable(Type parameter) =>
        IsNullableHelper(parameter, null, Enumerable.Empty<CustomAttributeData>());

    private bool IsNullableHelper(Type memberType, MemberInfo? declaringType, IEnumerable<CustomAttributeData> customAttributes)
    {
        if (memberType.IsValueType)
            return Nullable.GetUnderlyingType(memberType) != null;

        var nullable = customAttributes
            .FirstOrDefault(x => x.AttributeType.FullName == "System.Runtime.CompilerServices.NullableAttribute");
        if (nullable != null && nullable.ConstructorArguments.Count == 1)
        {
            var attributeArgument = nullable.ConstructorArguments[0];
            if (attributeArgument.ArgumentType == typeof(byte[]))
            {
                var args = (ReadOnlyCollection<CustomAttributeTypedArgument>)attributeArgument.Value!;
                if (args.Count > 0 && args[0].ArgumentType == typeof(byte))
                {
                    return (byte)args[0].Value! == 2;
                }
            }
            else if (attributeArgument.ArgumentType == typeof(byte))
            {
                return (byte)attributeArgument.Value! == 2;
            }
        }

        if(declaringType != null)
            for (var type = declaringType; type != null; type = type.DeclaringType)
            {
                var context = type.CustomAttributes
                    .FirstOrDefault(x => x.AttributeType.FullName == "System.Runtime.CompilerServices.NullableContextAttribute");
                if (context != null &&
                    context.ConstructorArguments.Count == 1 &&
                    context.ConstructorArguments[0].ArgumentType == typeof(byte))
                {
                    return (byte)context.ConstructorArguments[0].Value! == 2;
                }
            }

        return false;
    }

    public bool IsNumericType(Type type)
    {
        switch (Type.GetTypeCode(type))
        {
            case TypeCode.Byte:
            case TypeCode.SByte:
            case TypeCode.UInt16:
            case TypeCode.UInt32:
            case TypeCode.UInt64:
            case TypeCode.Int16:
            case TypeCode.Int32:
            case TypeCode.Int64:
            case TypeCode.Decimal:
            case TypeCode.Double:
            case TypeCode.Single:
                return true;
            default:
                return false;
        }
    }

    private string ResolveTypeName(Type type)
    {
        var isNullable = IsNullable(type);
        if (isNullable)
            type = type.GetGenericArguments()[0];

        string name = "NotImplemented";
        if (type == typeof(string))
            name = "string";
        else if (IsNumericType(type))
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
        var isNullable = IsNullable(type);
        var name = ResolveTypeName(type);
        if (name == string.Empty)
            throw new NotImplementedException($"Not implemented type: {type.Name}");

        return $"{name}{(isNullable ? " | null" : string.Empty)}";
    }

    private string ResolvePropertyInfoName(PropertyInfo propertyInfo)
    {
        var isNullable = IsNullable(propertyInfo);

        return $"{propertyInfo.Name.ToTypescriptName()}{(isNullable? "?" : "")}";
    }

    private string ResolveParameterInfoName(ParameterInfo parameterInfo)
    {
        var isNullable = IsNullable(parameterInfo);
        var name = parameterInfo.Name.ToTypescriptName();
        var typeName = ResolveTypescriptPropertyName(parameterInfo.ParameterType);
        return $"{name}{(isNullable ? "?" : "")}: {typeName}";
    }

    private string BuildTypescriptTypeForType(Type type)
    {
        var sb = new StringBuilder();
        var extends = type == typeof(object) ? "" : " extends Object";
        sb.AppendLine($"export interface {type.Name}{extends} {{");

        foreach (var propertyInfo in type.GetProperties())
        {
            var name = ResolvePropertyInfoName(propertyInfo);
            var typescriptType = ResolveTypescriptPropertyName(propertyInfo.PropertyType);
            sb.AppendLine($"  {name}: {typescriptType};");
        }
        
        foreach (var methodInfo in type.GetMethods()
            .Where(x => x.DeclaringType == type && !x.IsSpecialName))
        {
            var methodName = methodInfo.Name.ToTypescriptName();
            var parameters = string.Join(", ", methodInfo.GetParameters().Select(ResolveParameterInfoName));
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
