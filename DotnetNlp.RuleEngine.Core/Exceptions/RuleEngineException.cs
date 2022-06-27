using System;

namespace DotnetNlp.RuleEngine.Core.Exceptions;

/// <summary>
/// Represents the base class for all the exceptions that are intentionally thrown by the library.
/// </summary>
public abstract class RuleEngineException : Exception
{
    protected RuleEngineException(string message) : base(message)
    {
    }

    protected RuleEngineException(string message, Exception innerException) : base(message, innerException)
    {
    }
}