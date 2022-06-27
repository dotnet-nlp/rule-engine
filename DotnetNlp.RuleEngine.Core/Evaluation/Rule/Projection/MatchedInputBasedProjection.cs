using DotnetNlp.RuleEngine.Core.Evaluation.Rule.Projection.Arguments;
using DotnetNlp.RuleEngine.Core.Lib.Common.Helpers;

namespace DotnetNlp.RuleEngine.Core.Evaluation.Rule.Projection;

internal sealed class MatchedInputBasedProjection : IRuleProjection
{
    public static readonly MatchedInputBasedProjection Instance = new();

    private MatchedInputBasedProjection()
    {
    }

    public object? Invoke(ProjectionArguments projectionArguments)
    {
        return projectionArguments.Input.JoinToString(" ");
    }
}