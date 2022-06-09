using System;

namespace RuleEngine.Core.Lib.CodeAnalysis.Types.Resolving;

public sealed class TypeResolverException : Exception
{
    public TypeResolverException(string message) : base(message)
    {
    }
}