using RuleEngine.Core.Evaluation.Rule.Projection.Arguments;
using RuleEngine.Core.Lib.Common.Helpers;

namespace RuleEngine.Core.Evaluation.Rule.Projection;

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