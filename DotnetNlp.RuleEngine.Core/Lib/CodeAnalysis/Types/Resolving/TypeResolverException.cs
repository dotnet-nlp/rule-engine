using System;

namespace DotnetNlp.RuleEngine.Core.Lib.CodeAnalysis.Types.Resolving;

public sealed class TypeResolverException : Exception
{
    public TypeResolverException(string message) : base(message)
    {
    }
}