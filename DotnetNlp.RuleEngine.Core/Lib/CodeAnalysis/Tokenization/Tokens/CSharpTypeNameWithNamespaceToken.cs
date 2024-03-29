﻿namespace DotnetNlp.RuleEngine.Core.Lib.CodeAnalysis.Tokenization.Tokens;

public sealed class CSharpTypeNameWithNamespaceToken : IToken
{
    public string Value { get; }

    public CSharpTypeNameWithNamespaceToken(string value)
    {
        Value = value;
    }

    public override string ToString()
    {
        return Value;
    }
}