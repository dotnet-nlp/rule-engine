using DotnetNlp.RuleEngine.Core.Exceptions;

namespace DotnetNlp.RuleEngine.Mechanics.Peg.Exceptions;

public sealed class PegPatternTokenizationException : RuleEngineTokenizationException
{
    public PegPatternTokenizationException(string message, string source)
        : base(message, source)
    {
    }
}