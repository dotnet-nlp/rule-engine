using DotnetNlp.RuleEngine.Core.Evaluation.Rule.Input;
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
        RuleInput input,
        int firstSymbolIndex,
        RuleArguments ruleArguments,
        IRuleProjection projection
    )
    {
        var arguments = new ProjectionArguments(
            // todo [realtime performance] this must be slow, use Span<> or something
            input.Sequence[firstSymbolIndex..(result.LastUsedSymbolIndex + 1)],
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
            ruleArguments,
            input.RuleSpaceArguments
        );

        return projection.Invoke(arguments);
    }
}