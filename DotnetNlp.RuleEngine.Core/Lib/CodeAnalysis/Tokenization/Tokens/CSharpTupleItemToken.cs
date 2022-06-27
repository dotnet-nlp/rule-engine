namespace DotnetNlp.RuleEngine.Core.Lib.CodeAnalysis.Tokenization.Tokens;

public sealed class CSharpTupleItemToken : IToken
{
    public ICSharpTypeToken Type { get; }
    public CSharpIdentifierToken? PropertyName { get; }

    public CSharpTupleItemToken(ICSharpTypeToken type, CSharpIdentifierToken? propertyName)
    {
        Type = type;
        PropertyName = propertyName;
    }

    public override string ToString()
    {
        return $"{Type.ToString()}{(PropertyName is not null ? $" {PropertyName}" : "")}";
    }
}