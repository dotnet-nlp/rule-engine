using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using DotnetNlp.RuleEngine.Core.Evaluation.Rule;
using DotnetNlp.RuleEngine.Core.Evaluation.Rule.Projection.Parameters;
using DotnetNlp.RuleEngine.Core.Exceptions;

namespace DotnetNlp.RuleEngine.Core.Build.Rule.Static;

internal sealed class StaticRuleMatcherBuilder
{
    private readonly string _ruleName;
    private readonly Type _container;
    private readonly string _usedWordsProviderMethodName;
    private readonly MethodInfo _method;

    public StaticRuleMatcherBuilder(string ruleName, Type container, string usedWordsProviderMethodName, MethodInfo method)
    {
        _ruleName = ruleName;
        _container = container;
        _usedWordsProviderMethodName = usedWordsProviderMethodName;
        _method = method;
    }

    public IRuleMatcher Build<TResult>()
    {
        return new StaticRuleMatcher<TResult>(
            FindUsedWordsProvider(),
            CreateRuleEvaluator<TResult>(),
            DescribeRuleParameters()
        );
    }

    public static TReturn SwitchByType<TReturn>(
        MethodInfo method,
        Func<TReturn> single,
        Func<TReturn> multiple,
        string ruleName
    )
    {
        if (method.ReturnType.GetGenericTypeDefinition() == typeof(ValueTuple<,,>))
        {
            return single();
        }

        if (method.ReturnType.GetGenericTypeDefinition() == typeof(IEnumerable<>) &&
            method.ReturnType.GetGenericArguments().Single().GetGenericTypeDefinition() == typeof(ValueTuple<,>)
           )
        {
            return multiple();
        }

        throw new RuleBuildException(
            $"Return type of rule '{ruleName}' is not valid. " +
            $"Valid types are: " +
            $"'{typeof(ValueTuple<,,>).FullName}', " +
            $"'{typeof(IEnumerable<>).FullName}'. " +
            $"Value of type '{method.ReturnType.FullName}' given instead " +
            $"(class: {method.DeclaringType!.FullName}, method: {method.Name})."
        );
    }

    private static TReturn SwitchByType<TReturn, TResult>(
        MethodInfo method,
        Func<TReturn> single,
        Func<TReturn> multiple,
        string ruleName
    )
    {
        if (method.ReturnType == typeof(ValueTuple<bool, TResult, int>))
        {
            return single();
        }

        if (method.ReturnType == typeof(IEnumerable<ValueTuple<TResult, int>>))
        {
            return multiple();
        }

        throw new RuleBuildException(
            $"Return type of rule '{ruleName}' is not valid. " +
            $"Valid types are: " +
            $"'{typeof(ValueTuple<bool, TResult, int>).FullName}', " +
            $"'{typeof(IEnumerable<ValueTuple<TResult, int>>).FullName}'. " +
            $"Value of type '{method.ReturnType.FullName}' given instead " +
            $"(class: {method.DeclaringType!.FullName}, method: {method.Name})."
        );
    }

    private Func<IEnumerable<string>> FindUsedWordsProvider()
    {
        var methodInfo = _container.GetMethod(
            _usedWordsProviderMethodName,
            BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic
        );

        if (methodInfo is null)
        {
            throw new RuleBuildException(
                $"Used words provider for static rule '{_ruleName}' not found: " +
                $"function '{_container.FullName}.{_usedWordsProviderMethodName}' does not exist."
            );
        }

        if (methodInfo.GetParameters().Length > 0)
        {
            throw new RuleBuildException(
                $"Used words provider for static rule '{_ruleName}' is not valid: " +
                $"function '{_container.FullName}.{_usedWordsProviderMethodName}' should be parameterless."
            );
        }

        if (methodInfo.ReturnType != typeof(IEnumerable<string>))
        {
            throw new RuleBuildException(
                $"Used words provider for static rule '{_ruleName}' is not valid: " +
                $"function '{_container.FullName}.{_usedWordsProviderMethodName}' should return instance of {nameof(IEnumerable<string>)}."
            );
        }

        return () => (IEnumerable<string>) methodInfo.Invoke(null, Array.Empty<object>())!;
    }

    private Func<object?[], IEnumerable<(TResult Result, int LastUsedSymbolIndex)>> CreateRuleEvaluator<TResult>()
    {
        return SwitchByType<Func<object?[], IEnumerable<(TResult Result, int LastUsedSymbolIndex)>>, TResult>(
            _method,
            () => arguments => WrapSingleResultMethod<TResult>(_method, arguments),
            () => arguments => (IEnumerable<ValueTuple<TResult, int>>) _method.Invoke(null, arguments)!,
            _ruleName
        );
    }

    private static IEnumerable<(TResult Result, int LastUsedSymbolIndex)> WrapSingleResultMethod<TResult>(MethodInfo method, object?[] arguments)
    {
        (bool Success, TResult Result, int LastUsedSymbolIndex) result = ((bool, TResult, int)) method.Invoke(null, arguments)!;

        if (result.Success)
        {
            yield return (result.Result, result.LastUsedSymbolIndex);
        }
    }

    private RuleParameters DescribeRuleParameters()
    {
        if (!_method.IsStatic)
        {
            throw new RuleBuildException(
                $"Static rule '{_ruleName}' can only be built from static method " +
                $"(class: {_method.DeclaringType!.FullName}, method: {_method.Name})."
            );
        }

        var parameters = _method.GetParameters();

        const int constantParametersCount = 2;
        if (parameters.Length < constantParametersCount)
        {
            throw new RuleBuildException(
                $"Source method of rule '{_ruleName}' should contain " +
                $"at least two parameters 'string[] sequence' and 'int startIndex' " +
                $"(class: {_method.DeclaringType!.FullName}, method: {_method.Name})."
            );
        }

        const int sequenceParameterIndex = 0;
        var sequenceParameter = parameters[sequenceParameterIndex];

        if (sequenceParameter.ParameterType != typeof(string[]))
        {
            throw new RuleBuildException(
                $"Parameter '{sequenceParameterIndex}' of the source method of rule '{_ruleName}' " +
                $"should be 'string[] sequence', " +
                $"'{sequenceParameter.ParameterType.FullName} {sequenceParameter.Name}' given instead " +
                $"(class: {_method.DeclaringType!.FullName}, method: {_method.Name})."
            );
        }

        const int startIndexParameterIndex = 1;
        var startIndexParameter = parameters[startIndexParameterIndex];

        if (startIndexParameter.ParameterType != typeof(int))
        {
            throw new RuleBuildException(
                $"Parameter '{startIndexParameterIndex}' of the source method of rule '{_ruleName}' " +
                $"should be 'int startIndex', " +
                $"'{startIndexParameter.ParameterType.FullName} {startIndexParameter.Name}' given instead " +
                $"(class: {_method.DeclaringType!.FullName}, method: {_method.Name})."
            );
        }

        return new RuleParameters(
            parameters
                .Skip(constantParametersCount)
                .ToDictionary(parameter => parameter.Name!, parameter => parameter.ParameterType)
        );
    }
}