using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace DotnetNlp.RuleEngine.Core.Lib.CodeAnalysis.Types.Formatting;

public sealed class TypeFormatter : ITypeFormatter
{
    public string GetStringRepresentation(Type type)
    {
        if (type.FullName is null || type.FullName.Length == 0)
        {
            throw new TypeFormatterException("Cannot build type with empty name");
        }

        var result = new StringBuilder(type.FullName.Length);

        BuildCSharpFullName(type, result, new List<Type>(type.GetTypeInfo().GenericTypeArguments));

        return result.ToString();
    }

    private static void BuildCSharpFullName(Type type, StringBuilder result, List<Type> typeArguments)
    {
        var typeInfo = type.GetTypeInfo();

        var localTypeParametersCount = typeInfo.GenericTypeParameters.Length;
        var localTypeArgumentsCount = typeInfo.GenericTypeArguments.Length;

        if (type.DeclaringType is not null)
        {
            BuildCSharpFullName(type.DeclaringType, result, typeArguments);
        }
        else
        {
            result.Append(type.Namespace);
        }

        result.Append('.');

        foreach (var c in type.Name.TakeWhile(c => c != '`'))
        {
            result.Append(c);
        }

        if (localTypeParametersCount > 0)
        {
            result.Append('<');

            for (var i = 0; i < localTypeParametersCount; i++)
            {
                if (i > 0)
                {
                    result.Append(',');
                }

                BuildCSharpFullName(typeArguments[i], result, new List<Type>(typeArguments[i].GetTypeInfo().GenericTypeArguments));
            }

            typeArguments.RemoveRange(0, localTypeParametersCount);

            result.Append('>');
        }
        else if (localTypeArgumentsCount > 0 && typeArguments.Count > 0)
        {
            result.Append('<');

            for (var i = 0; i < Math.Min(localTypeArgumentsCount, typeArguments.Count); i++)
            {
                if (i > 0)
                {
                    result.Append(',');
                }

                BuildCSharpFullName(typeArguments[i], result, new List<Type>(typeArguments[i].GetTypeInfo().GenericTypeArguments));
            }

            result.Append('>');
        }
    }
}