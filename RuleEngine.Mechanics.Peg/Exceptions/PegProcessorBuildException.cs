using RuleEngine.Core.Exceptions;

namespace RuleEngine.Mechanics.Peg.Exceptions;

public sealed class PegProcessorBuildException : RuleBuildException
{
    public PegProcessorBuildException(string message) : base(message)
    {
    }
}