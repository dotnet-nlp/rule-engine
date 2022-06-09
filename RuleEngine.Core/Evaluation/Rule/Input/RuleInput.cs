using RuleEngine.Core.Evaluation.Rule.Projection.Arguments;

namespace RuleEngine.Core.Evaluation.Rule.Input;

/// <remarks>
/// Performance remarks: library performance depends on the way the fields in this class are declared.
/// Please make sure you know what you are doing, when changing any of the fields declaration.
/// </remarks>
public sealed class RuleInput
{
    public readonly string[] Sequence;
    public readonly RuleSpaceArguments RuleSpaceArguments;

    public RuleInput(string[] input, RuleSpaceArguments ruleSpaceArguments)
    {
        Sequence = input;
        RuleSpaceArguments = ruleSpaceArguments;
    }
}