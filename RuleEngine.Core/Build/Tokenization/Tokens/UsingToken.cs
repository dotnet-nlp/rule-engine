using RuleEngine.Core.Lib.CodeAnalysis.Tokenization.Tokens;

namespace RuleEngine.Core.Build.Tokenization.Tokens;

public sealed class UsingToken : IToken
{
    public string Namespace { get; }

    public UsingToken(string @namespace)
    {
        Namespace = @namespace;
    }

    public override string ToString()
    {
        return $"using {Namespace};";
    }
}