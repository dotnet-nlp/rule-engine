using RuleEngine.Core.Lib.CodeAnalysis.Tokenization.Tokens;

namespace RuleEngine.Core.Build.Tokenization.Tokens;

public interface IRuleToken : IToken
{
    string? Namespace { get; }
    string Name { get; }
    ICSharpTypeToken ReturnType { get; }
    CSharpParameterToken[] RuleParameters { get; }
    IProjectionToken Projection { get; }
}

public static class RuleTokenExtensions
{
    public static string GetFullName(this IRuleToken ruleToken)
    {
        return ruleToken.Namespace is null ? ruleToken.Name : $"{ruleToken.Namespace}.{ruleToken.Name}";
    }
}