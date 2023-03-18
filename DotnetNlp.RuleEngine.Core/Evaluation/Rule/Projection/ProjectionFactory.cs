using DotnetNlp.RuleEngine.Core.Evaluation.Rule.Projection.Arguments;
using DotnetNlp.RuleEngine.Core.Evaluation.Rule.Projection.Parameters;
using DotnetNlp.RuleEngine.Core.Evaluation.Rule.Result;
using DotnetNlp.RuleEngine.Core.Lib.Common.Helpers;

namespace DotnetNlp.RuleEngine.Core.Evaluation.Rule.Projection;

internal static class ProjectionFactory
{
    public static object? GetProjectionResult(
        RuleMatchResult result,
        CapturedVariablesParameters capturedVariablesParameters,
        string[] sequence,
        IRuleProjection projection,
        int firstSymbolIndex = 0,
        RuleSpaceArguments? ruleSpaceArguments = null,
        RuleArguments? ruleArguments = null
    )
    {
        var arguments = new ProjectionArguments(
            // todo [realtime performance] this must be slow, use Span<> or something
            sequence[firstSymbolIndex..(result.LastUsedSymbolIndex + 1)],
            new CapturedVariablesArguments(
                capturedVariablesParameters
                    .Values
                    .MapValue(
                        (parameterName, _) =>
                        {
                            if (result.CapturedVariables is null)
                            {
                                return null;
                            }

                            return result.CapturedVariables.TryGetValue(parameterName, out var capturedVariable)
                                ? capturedVariable
                                : null;
                        }
                    )
                    .ToDictionaryWithKnownCapacity(capturedVariablesParameters.Values.Count)
            ),
            ruleArguments ?? RuleArguments.Empty,
            ruleSpaceArguments ?? RuleSpaceArguments.Empty
        );

        return projection.Invoke(arguments);
    }
}