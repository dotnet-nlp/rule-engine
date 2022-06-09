using RuleEngine.Core.Exceptions;

namespace RuleEngine.Mechanics.Regex.Exceptions;

public sealed class RegexPatternTokenizationException : RuleEngineTokenizationException
{
    public RegexPatternTokenizationException(string message, string source)
        : base(message, source)
    {
    }
}