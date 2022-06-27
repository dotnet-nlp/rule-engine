using System;
using System.Collections.Generic;
using DotnetNlp.RuleEngine.Core.Evaluation.Rule.Projection.Arguments;
using DotnetNlp.RuleEngine.Core.Evaluation.Rule.Projection.Parameters;
using DotnetNlp.RuleEngine.Core.Exceptions;

namespace DotnetNlp.RuleEngine.Core.Evaluation.Rule.Projection;

internal sealed class FunctionBasedProjection : IRuleProjection
{
    private readonly ProjectionParameters _parameters;
    private readonly Func<object?[], object?> _function;

    public FunctionBasedProjection(ProjectionParameters parameters, Func<object?[], object?> function)
    {
        _parameters = parameters;
        _function = function;
    }

    public object? Invoke(ProjectionArguments projectionArguments)
    {
        var arguments = new List<object?>(_parameters.Values.Count);

        foreach (var (variableName, type) in _parameters.Values)
        {
            // todo [refactoring] generalize this idea of void rules
            if (type == typeof(void))
            {
                continue;
            }

            arguments.Add(projectionArguments.Values[variableName]);
        }

        return TryInvokeProjection(arguments.ToArray());
    }

    private object? TryInvokeProjection(object?[] arguments)
    {
        try
        {
            return _function.Invoke(arguments);
        }
        catch (Exception exception)
        {
            throw new RuleMatchException("Projection invocation error", exception);
        }
    }
}