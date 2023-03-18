using System;
using System.Collections.Generic;
using System.Linq;
using DotnetNlp.RuleEngine.Core.Build.Tokenization.Tokens.Arguments;
using DotnetNlp.RuleEngine.Core.Evaluation.Rule.Projection.Arguments;
using DotnetNlp.RuleEngine.Core.Exceptions;
using DotnetNlp.RuleEngine.Core.Lib.Common.Helpers;

namespace DotnetNlp.RuleEngine.Core.Evaluation.Rule.Projection.Parameters;

public sealed class RuleParameters
{
    public static readonly RuleParameters Empty = new(Array.Empty<KeyValuePair<string, Type>>());

    public KeyValuePair<string, Type>[] Values { get; }

    public RuleParameters(KeyValuePair<string, Type>[] values)
    {
        Values = values;
    }

    public RuleArguments? BindRuleArguments(
        IRuleArgumentToken[]? arguments,
        RuleSpaceArguments? ruleSpaceArguments
    )
    {
        if (Values.Length == 0)
        {
            if (arguments is not null)
            {
                throw new RuleMatchException("Rule has given arguments, but none are required.");
            }

            return null;
        }

        return new RuleArguments(
            Values
                .MapValue(
                    (index, _, parameterType) =>
                    {
                        if (index < arguments?.Length)
                        {
                            var argumentToken = arguments[index];

                            return argumentToken switch
                            {
                                RuleDefaultArgumentToken => GetDefaultValue(parameterType),
                                ChainedMemberAccessArgumentToken token => GetValueFromRuleSpace(
                                    token,
                                    parameterType,
                                    ruleSpaceArguments
                                ),
                                _ => throw new ArgumentOutOfRangeException(nameof(argumentToken)),
                            };
                        }

                        return GetDefaultValue(parameterType);
                    }
                )
                .ToArray()
        );
    }

    private static object? GetValueFromRuleSpace(
        ChainedMemberAccessArgumentToken argumentToken,
        Type expectedArgumentType,
        RuleSpaceArguments? ruleSpaceArguments
    )
    {
        if (ruleSpaceArguments is null)
        {
            throw new RuleMatchException(
                $"Unable to get value of argument '{argumentToken}': rule space arguments are not provided."
            );
        }

        var argument = ruleSpaceArguments.GetArgument(argumentToken);

        if (argument is null)
        {
            if (expectedArgumentType.IsValueType && !expectedArgumentType.IsNullable())
            {
                throw new RuleMatchException(
                    $"Error getting value of argument '{argumentToken}': " +
                    $"extracted value is null, while expected value type is '{expectedArgumentType}'."
                );
            }
        }
        else if (!argument.GetType().IsAssignableTo(expectedArgumentType))
        {
            throw new RuleMatchException(
                $"Error getting value of argument '{argumentToken}': " +
                $"extracted value is of type '{argument.GetType()}', " +
                $"which isn't assignable to expected value type '{expectedArgumentType}'."
            );
        }

        return argument;
    }

    private static object? GetDefaultValue(Type type)
    {
        return type.IsValueType ? Activator.CreateInstance(type) : null;
    }
}