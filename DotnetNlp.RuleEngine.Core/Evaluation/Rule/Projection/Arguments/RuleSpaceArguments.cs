using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using DotnetNlp.RuleEngine.Core.Build.Tokenization.Tokens.Arguments;
using DotnetNlp.RuleEngine.Core.Exceptions;
using DotnetNlp.RuleEngine.Core.Lib.Common.Helpers;

namespace DotnetNlp.RuleEngine.Core.Evaluation.Rule.Projection.Arguments;

public sealed class RuleSpaceArguments
{
    public static readonly RuleSpaceArguments Empty = new(ImmutableDictionary<string, object?>.Empty);

    /// <remarks>
    /// Performance remarks: library performance depends on the way this field is declared.
    /// Please make sure you know what you are doing, when changing this field's declaration.
    /// </remarks>
    public readonly IReadOnlyDictionary<string, object?> Values;

    public RuleSpaceArguments(IReadOnlyDictionary<string, object?> values)
    {
        Values = values;
    }

    public IReadOnlyDictionary<string, object?>? GetValues(IReadOnlySet<IChainedMemberAccessToken>? parameters)
    {
        return parameters?.ToDictionary(parameter => parameter.ToString(), GetArgument);
    }

    public object? GetArgument(IChainedMemberAccessToken argumentToken)
    {
        var formattedCallChain = argumentToken.CallChain.JoinToString(".");

        var rootObjectName = argumentToken.CallChain.First();
        if (!Values.TryGetValue(rootObjectName, out var rootObject))
        {
            throw new RuleMatchException(
                $"Object '{rootObjectName}' is not part of rule space arguments " +
                $"(call chain: {formattedCallChain})."
            );
        }

        var targetObject = rootObject;
        var targetObjectName = rootObjectName;
        foreach (var memberName in argumentToken.CallChain.Skip(1))
        {
            targetObject = GetFromObject(targetObject, targetObjectName, memberName, formattedCallChain);
            targetObjectName = memberName;
        }

        return targetObject;
    }

    private static object? GetFromObject(object? instance, string instanceName, string memberName, string formattedCallChain)
    {
        if (instance is null)
        {
            throw new RuleMatchException(
                $"Object '{instanceName}' is null " +
                $"and cannot be used as a source of {nameof(ChainedMemberAccessArgumentToken)} binding " +
                $"(call chain: {formattedCallChain})."
            );
        }

        var property = instance.GetType().GetProperty(memberName);

        if (property is null)
        {
            throw new RuleMatchException(
                $"Object '{instanceName}' does not contain property {memberName} " +
                $"(call chain: {formattedCallChain})."
            );
        }

        return property.GetValue(instance);
    }
}