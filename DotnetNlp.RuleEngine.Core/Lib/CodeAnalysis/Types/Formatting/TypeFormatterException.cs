using System;

namespace DotnetNlp.RuleEngine.Core.Lib.CodeAnalysis.Types.Formatting;

public sealed class TypeFormatterException : Exception
{
    public TypeFormatterException(string message) : base(message)
    {
    }
}