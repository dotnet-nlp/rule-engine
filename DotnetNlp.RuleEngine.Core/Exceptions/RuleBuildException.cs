using System;

namespace DotnetNlp.RuleEngine.Core.Exceptions;

public class RuleBuildException : RuleEngineException
{
    public RuleBuildException(string message) : base(message)
    {
    }

    public RuleBuildException(string message, Exception innerException) : base(message, innerException)
    {
    }
}