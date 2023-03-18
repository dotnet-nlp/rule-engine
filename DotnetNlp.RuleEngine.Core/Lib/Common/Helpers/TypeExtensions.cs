using System;

namespace DotnetNlp.RuleEngine.Core.Lib.Common.Helpers;

public static class TypeExtensions
{
    public static bool IsNullable(this Type type)
    {
        return type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Nullable<>);
    }
}