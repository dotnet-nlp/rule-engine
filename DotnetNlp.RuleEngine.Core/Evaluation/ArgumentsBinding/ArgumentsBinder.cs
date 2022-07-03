using System;
using System.Linq;
using DotnetNlp.RuleEngine.Core.Build.Tokenization.Tokens.Arguments;
using DotnetNlp.RuleEngine.Core.Evaluation.Rule.Projection.Arguments;
using DotnetNlp.RuleEngine.Core.Evaluation.Rule.Projection.Parameters;
using DotnetNlp.RuleEngine.Core.Exceptions;
using DotnetNlp.RuleEngine.Core.Lib.Common.Helpers;

namespace DotnetNlp.RuleEngine.Core.Evaluation.ArgumentsBinding;

public static class ArgumentsBinder
{
    public static RuleArguments BindRuleArguments(
        RuleParameters ruleParameters,
        IRuleArgumentToken[] argumentBindings,
        RuleSpaceArguments? ruleSpaceArguments
    )
    {
        return new RuleArguments(
            ruleParameters
                .Values
                .MapValue(
                    (index, _, parameterType) =>
                    {
                        if (index < argumentBindings.Length)
                        {
                            var argument = argumentBindings[index];

                            return argument switch
                            {
                                RuleDefaultArgumentToken => GetDefaultValue(parameterType),
                                RuleChainedMemberAccessArgumentToken binding => GetFromRuleSpace(binding, ruleSpaceArguments),
                                _ => throw new ArgumentOutOfRangeException(nameof(argument)),
                            };
                        }

                        return GetDefaultValue(parameterType);
                    }
                )
                .ToDictionaryWithKnownCapacity(ruleParameters.Values.Count)
        );
    }

    private static object? GetDefaultValue(Type type)
    {
        return type.IsValueType ? Activator.CreateInstance(type) : null;
    }

    private static object? GetFromRuleSpace(
        RuleChainedMemberAccessArgumentToken binding,
        RuleSpaceArguments? ruleSpaceArguments
    )
    {
        var formattedCallChain = binding.CallChain.JoinToString(".");

        var rootObjectName = binding.CallChain.First();
        if (!(ruleSpaceArguments?.Values.TryGetValue(rootObjectName, out var rootObject) ?? false))
        {
            throw new RuleMatchException(
                $"Object '{rootObjectName}' is not part of rule space arguments " +
                $"(call chain: {formattedCallChain})."
            );
        }

        var targetObject = rootObject;
        var targetObjectName = rootObjectName;
        foreach (var memberName in binding.CallChain.Skip(1))
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
                $"Object '{instance}' is null " +
                $"and cannot be used as a source of {nameof(RuleChainedMemberAccessArgumentToken)} binding " +
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