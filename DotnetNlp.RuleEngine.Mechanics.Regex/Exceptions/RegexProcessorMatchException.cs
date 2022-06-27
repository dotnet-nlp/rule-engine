using System;
using DotnetNlp.RuleEngine.Core.Exceptions;

namespace DotnetNlp.RuleEngine.Mechanics.Regex.Exceptions;

public sealed class RegexProcessorMatchException : RuleMatchException
{
    public RegexProcessorMatchException(string message) : base(message)
    {
    }

    public RegexProcessorMatchException(string message, Exception innerException) : base(message, innerException)
    {
    }
}