using DotnetNlp.RuleEngine.Core.Evaluation.Rule.Projection.Arguments;

namespace DotnetNlp.RuleEngine.Core.Evaluation.Rule.Projection;

internal interface IRuleProjection
{
    object? Invoke(ProjectionArguments projectionArguments);
}