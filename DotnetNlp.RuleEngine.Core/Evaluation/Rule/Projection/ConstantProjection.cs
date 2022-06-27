using DotnetNlp.RuleEngine.Core.Evaluation.Rule.Projection.Arguments;

namespace DotnetNlp.RuleEngine.Core.Evaluation.Rule.Projection;

internal sealed class ConstantProjection : IRuleProjection
{
    private readonly object? _constant;

    public ConstantProjection(object? constant)
    {
        _constant = constant;
    }

    public object? Invoke(ProjectionArguments projectionArguments)
    {
        return _constant;
    }
}