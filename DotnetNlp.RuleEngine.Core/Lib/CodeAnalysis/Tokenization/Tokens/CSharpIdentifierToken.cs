namespace DotnetNlp.RuleEngine.Core.Lib.CodeAnalysis.Tokenization.Tokens;

public sealed class CSharpIdentifierToken : IToken
{
    public string Value { get; }

    public CSharpIdentifierToken(string value)
    {
        Value = value;
    }

    public override string ToString()
    {
        return Value;
    }
}