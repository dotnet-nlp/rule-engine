using System;
using DotnetNlp.RuleEngine.Core.Exceptions;

namespace DotnetNlp.RuleEngine.Mechanics.Regex.Exceptions;

public sealed class RegexProcessorBuildException : RuleBuildException
{
    public RegexProcessorBuildException(string message) : base(message)
    {
    }

    public RegexProcessorBuildException(string message, Exception innerException) : base(message, innerException)
    {
    }
}