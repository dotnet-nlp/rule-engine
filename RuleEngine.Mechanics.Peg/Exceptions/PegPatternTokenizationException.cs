using RuleEngine.Core.Exceptions;

namespace RuleEngine.Mechanics.Peg.Exceptions;

public sealed class PegPatternTokenizationException : RuleEngineTokenizationException
{
    public PegPatternTokenizationException(string message, string source)
        : base(message, source)
    {
    }
}