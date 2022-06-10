using System;
using RuleEngine.Core.Lib.Common.Helpers;

namespace RuleEngine.Core.Exceptions;

public class RuleEngineTokenizationException : RuleEngineException
{
    private static readonly ErrorIndexHelper ErrorIndexHelper = new ErrorIndexHelper(Environment.NewLine);

    public RuleEngineTokenizationException(string message, Exception innerException, string source) : base(message, innerException)
    {
        ErrorIndexHelper.FillExceptionData(Data, source);
    }

    public RuleEngineTokenizationException(string message, string source) : base(message)
    {
        ErrorIndexHelper.FillExceptionData(Data, source);
    }
}