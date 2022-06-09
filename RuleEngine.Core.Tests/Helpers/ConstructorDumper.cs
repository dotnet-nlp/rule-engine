using System;
using System.Collections;
using System.Linq;
using System.Reflection;
using RuleEngine.Core.Lib.Common.Helpers;

namespace RuleEngine.Core.Tests.Helpers;

internal sealed class ConstructorDumper
{
    private readonly int _indentLevelSize;
    private readonly char _indentCharacter;
    private readonly int _rowLengthLimit;

    public ConstructorDumper(int indentLevelSize = 4, char indentCharacter = ' ', int rowLengthLimit = 120)
    {
        _indentLevelSize = indentLevelSize;
        _indentCharacter = indentCharacter;
        _rowLengthLimit = rowLengthLimit;
    }

    public string Dump(object? @object, int indent = 0)
    {
        var indentString = GetIndentString(indent);

        if (@object is null)
        {
            return $"{indentString}null";
        }

        var knownTypeDump = DumpKnownType(@object, indent);

        if (knownTypeDump is not null)
        {
            return knownTypeDump;
        }

        var objectType = @object.GetType();

        var constructor = objectType.GetConstructors().SingleOrDefault();

        if (constructor is null)
        {
            if (objectType.GetField("Instance", BindingFlags.Static | BindingFlags.Public) is null)
            {
                throw new Exception($"Cannot dump instance of {objectType.FullName}.");
            }

            return $"{indentString}{objectType.Name}.Instance";
        }

        var constructorParameters = constructor.GetParameters();

        var constructorArguments = constructorParameters
            .Select(parameter => parameter.Name!)
            .Select(parameterName => parameterName.Capitalize())
            .Select(propertyName => objectType.GetProperty(propertyName)!.GetValue(@object))
            .ToArray();

        var formattedArgumentsJoinedWithSpace = constructorArguments
            .Select(argument => Dump(argument))
            .JoinToString(", ");

        if (!formattedArgumentsJoinedWithSpace.Contains(Environment.NewLine))
        {
            var formattedConstructorCall = $"{indentString}new {objectType.Name}({formattedArgumentsJoinedWithSpace})";

            if (formattedConstructorCall.Length <= _rowLengthLimit)
            {
                return formattedConstructorCall;
            }
        }

        return $"{indentString}new {objectType.Name}({Environment.NewLine}" +
               $"{constructorArguments.Select(argument => Dump(argument, indent + _indentLevelSize)).JoinToString($",{Environment.NewLine}")}{Environment.NewLine}" +
               $"{indentString})";
    }

    private string? DumpKnownType(object @object, int indent)
    {
        var indentString = GetIndentString(indent);

        var keywordTypeDump = @object switch
        {
            string value => value.Contains('"') || value.Contains(Environment.NewLine) ? $"@\"{value.Replace("\"", "\"\"")}\"" : $"\"{value}\"",
            bool value => value ? "true" : "false",
            byte value => $"(byte) {value}",
            sbyte value => $"(sbyte) {value}",
            char value => $"'{value}'",
            decimal value => $"{value}m",
            double value => $"{value}d",
            float value => $"{value}f",
            int value => $"{value}",
            uint value => $"{value}u",
            long value => $"{value}l",
            ulong value => $"{value}ul",
            short value => $"(short) {value}",
            ushort value => $"(ushort) {value}",
            _ => null,
        };

        if (keywordTypeDump is not null)
        {
            return $"{indentString}{keywordTypeDump}";
        }

        if (@object is IEnumerable enumerable)
        {
            var objectType = enumerable.GetType();

            if (objectType.HasElementType)
            {
                var formattedItems = (from object item in enumerable select Dump(item, indent + _indentLevelSize)).ToArray();

                if (formattedItems.Length == 0)
                {
                    return $"{indentString}Array.Empty<{objectType.GetElementType()!.Name}>()";
                }

                return $"{indentString}new []{Environment.NewLine}" +
                       $"{indentString}{{{Environment.NewLine}" +
                       $"{formattedItems.JoinToString($",{Environment.NewLine}")}{Environment.NewLine}" +
                       $"{indentString}}}";
            }
        }

        return null;
    }

    private string GetIndentString(int indentSize)
    {
        return new string(_indentCharacter, indentSize);
    }
}