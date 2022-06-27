using System;
using DotnetNlp.RuleEngine.Core.Exceptions;

namespace DotnetNlp.RuleEngine.Mechanics.Peg.Exceptions;

public sealed class PegProcessorMatchException : RuleMatchException
{
    public PegProcessorMatchException(string message) : base(message)
    {
    }

    public PegProcessorMatchException(string message, Exception innerException) : base(message, innerException)
    {
    }
}