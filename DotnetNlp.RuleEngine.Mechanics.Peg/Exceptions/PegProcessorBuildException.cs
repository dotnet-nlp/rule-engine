using DotnetNlp.RuleEngine.Core.Exceptions;

namespace DotnetNlp.RuleEngine.Mechanics.Peg.Exceptions;

public sealed class PegProcessorBuildException : RuleBuildException
{
    public PegProcessorBuildException(string message) : base(message)
    {
    }
}