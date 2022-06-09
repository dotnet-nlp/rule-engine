using RuleEngine.Core.Evaluation.Rule.Projection.Arguments;

namespace RuleEngine.Core.Evaluation.Rule.Projection;

internal sealed class VoidProjection : IRuleProjection
{
    public static readonly VoidProjection Instance = new();

    private VoidProjection()
    {
    }

    public object? Invoke(ProjectionArguments projectionArguments)
    {
        return null;
    }
}