using DotnetNlp.RuleEngine.Core.Lib.CodeAnalysis.Tokenization.Tokens;

namespace DotnetNlp.RuleEngine.Core.Build.Tokenization.Tokens;

public sealed class VoidProjectionToken : IProjectionToken
{
    public static readonly VoidProjectionToken Instance = new();
    public static readonly ICSharpTypeToken ReturnType = new ResolvedCSharpTypeToken("void", typeof(void));

    private VoidProjectionToken()
    {
    }

    public override string ToString()
    {
        return "{}";
    }
}