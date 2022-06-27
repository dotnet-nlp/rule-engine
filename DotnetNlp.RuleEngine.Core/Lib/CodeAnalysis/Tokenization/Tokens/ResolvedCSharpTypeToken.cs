using System;

namespace DotnetNlp.RuleEngine.Core.Lib.CodeAnalysis.Tokenization.Tokens;

public sealed class ResolvedCSharpTypeToken : ICSharpTypeToken
{
    public string TypeDeclaration { get; }
    public Type Type { get; }

    public ResolvedCSharpTypeToken(string typeDeclaration, Type type)
    {
        TypeDeclaration = typeDeclaration;
        Type = type;
    }

    public override string ToString()
    {
        return TypeDeclaration;
    }
}