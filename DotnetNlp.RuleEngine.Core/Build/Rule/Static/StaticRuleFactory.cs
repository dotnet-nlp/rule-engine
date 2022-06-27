using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using DotnetNlp.RuleEngine.Core.Build.Rule.Static.Attributes;
using DotnetNlp.RuleEngine.Core.Evaluation.Rule;
using DotnetNlp.RuleEngine.Core.Exceptions;
using DotnetNlp.RuleEngine.Core.Lib.Common.Helpers;

namespace DotnetNlp.RuleEngine.Core.Build.Rule.Static;

public sealed class StaticRuleFactory
{
    private readonly RuleSpaceFactory _ruleSpaceFactory;

    public StaticRuleFactory(RuleSpaceFactory ruleSpaceFactory)
    {
        _ruleSpaceFactory = ruleSpaceFactory;
    }

    public Dictionary<string, IRuleMatcher> ConvertStaticRuleContainerToRuleMatchers(Type container)
    {
        var containerAttribute = container.GetCustomAttribute<StaticRuleContainerAttribute>();

        if (containerAttribute is null)
        {
            throw new RuleBuildException(
                $"Type {container.FullName} is not a valid static rule container: " +
                $"{nameof(StaticRuleContainerAttribute)} is missing"
            );
        }

        var ruleNamespace = containerAttribute.Namespace;

        return container
            .GetMethods(BindingFlags.Static | BindingFlags.Public)
            .Select(methodInfo => (Method: methodInfo, Attribute: methodInfo.GetCustomAttribute<StaticRuleAttribute>()))
            .Where(data => data.Attribute is not null)
            .Select(
                tuple =>
                {
                    var ruleName = $"{ruleNamespace}.{tuple.Attribute!.Name}";

                    return new KeyValuePair<string, IRuleMatcher>(
                        ruleName,
                        (IRuleMatcher) typeof(StaticRuleMatcherBuilder)
                            .GetMethod(nameof(StaticRuleMatcherBuilder.Build))!
                            .MakeGenericMethod(DescribeResultType(tuple.Method, ruleName))
                            .Invoke(
                                new StaticRuleMatcherBuilder(
                                    ruleName,
                                    container,
                                    tuple.Attribute.UsedWordsProviderMethodName,
                                    tuple.Method
                                ),
                                Array.Empty<object>()
                            )!
                    );
                }
            )
            .MapValue(ruleMatcher => (IRuleMatcher) _ruleSpaceFactory.WrapWithCache(ruleMatcher))
            .ToDictionary();
    }

    private static Type DescribeResultType(MethodInfo method, string ruleName)
    {
        var returnType = method.ReturnType;

        return StaticRuleMatcherBuilder.SwitchByType(
            method,
            () =>
            {
                const int successArgumentIndex = 0;
                var successArgument = returnType.GenericTypeArguments[successArgumentIndex];
                if (returnType.GenericTypeArguments[successArgumentIndex] != typeof(bool))
                {
                    throw new RuleBuildException(
                        $"Return type of rule '{ruleName}' is not valid: tuple element '{successArgumentIndex}' " +
                        $"should be 'bool success', '{successArgument.FullName} given instead " +
                        $"(class: {method.DeclaringType!.FullName}, method: {method.Name})."
                    );
                }

                const int lastUsedSymbolIndexArgumentIndex = 2;
                var lastUsedSymbolIndexArgument =
                    returnType.GenericTypeArguments[lastUsedSymbolIndexArgumentIndex];
                if (returnType.GenericTypeArguments[lastUsedSymbolIndexArgumentIndex] != typeof(int))
                {
                    throw new RuleBuildException(
                        $"Return type of rule '{ruleName}' is not valid: tuple element '{lastUsedSymbolIndexArgumentIndex}' " +
                        $"should be 'int lastUsedSymbolIndex', '{lastUsedSymbolIndexArgument.FullName} given instead " +
                        $"(class: {method.DeclaringType!.FullName}, method: {method.Name})."
                    );
                }

                return returnType.GenericTypeArguments[1];
            },
            () =>
            {
                var itemType = returnType.GetGenericArguments().Single();

                const int lastUsedSymbolIndexArgumentIndex = 1;
                var lastUsedSymbolIndexArgument = itemType.GenericTypeArguments[lastUsedSymbolIndexArgumentIndex];
                if (itemType.GenericTypeArguments[lastUsedSymbolIndexArgumentIndex] != typeof(int))
                {
                    throw new RuleBuildException(
                        $"Return type of rule '{ruleName}' is not valid: tuple element '{lastUsedSymbolIndexArgumentIndex}' " +
                        $"should be 'int lastUsedSymbolIndex', '{lastUsedSymbolIndexArgument.FullName} given instead " +
                        $"(class: {method.DeclaringType!.FullName}, method: {method.Name})."
                    );
                }

                return itemType.GenericTypeArguments[0];
            },
            ruleName
        );
    }
}