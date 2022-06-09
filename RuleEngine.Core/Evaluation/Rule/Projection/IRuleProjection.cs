using RuleEngine.Core.Evaluation.Rule.Projection.Arguments;

namespace RuleEngine.Core.Evaluation.Rule.Projection;

internal interface IRuleProjection
{
    object? Invoke(ProjectionArguments projectionArguments);
}