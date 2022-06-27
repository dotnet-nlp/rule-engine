using DotnetNlp.RuleEngine.Core.Lib.CodeAnalysis.Tokenization.Tokens;

namespace DotnetNlp.RuleEngine.Core.Build.Tokenization.Tokens;

public sealed class MatchedInputBasedProjectionToken : IProjectionToken
{
    public static readonly MatchedInputBasedProjectionToken Instance = new();
    public static readonly ICSharpTypeToken ReturnType = new ResolvedCSharpTypeToken("string", typeof(string));

    private MatchedInputBasedProjectionToken()
    {
    }

    public override string ToString()
    {
        return "{ return input; }";
    }
}