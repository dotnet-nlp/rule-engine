using System;

namespace RuleEngine.Core.Exceptions;

public class RuleMatchException : RuleEngineException
{
    public RuleMatchException(string message) : base(message)
    {
    }

    public RuleMatchException(string message, Exception innerException) : base(message, innerException)
    {
    }
}