namespace DotnetNlp.RuleEngine.Core.Lib.CodeAnalysis.Tokenization.Tokens;

public sealed class CSharpParameterToken : IToken
{
    public ICSharpTypeToken Type { get; }
    public string Name { get; }

    public CSharpParameterToken(ICSharpTypeToken type, string name)
    {
        Type = type;
        Name = name;
    }

    public override string ToString()
    {
        return $"{Type.ToString()} {Name}";
    }
}