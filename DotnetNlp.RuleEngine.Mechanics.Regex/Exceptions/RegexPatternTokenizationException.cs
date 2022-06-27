using DotnetNlp.RuleEngine.Core.Exceptions;

namespace DotnetNlp.RuleEngine.Mechanics.Regex.Exceptions;

public sealed class RegexPatternTokenizationException : RuleEngineTokenizationException
{
    public RegexPatternTokenizationException(string message, string source)
        : base(message, source)
    {
    }
}