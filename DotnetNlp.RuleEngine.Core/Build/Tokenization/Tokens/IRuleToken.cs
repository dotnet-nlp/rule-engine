using DotnetNlp.RuleEngine.Core.Lib.CodeAnalysis.Tokenization.Tokens;

namespace DotnetNlp.RuleEngine.Core.Build.Tokenization.Tokens;

public interface IRuleToken : IToken
{
    string? Namespace { get; }
    string Name { get; }
    ICSharpTypeToken ReturnType { get; }
    CSharpParameterToken[] RuleParameters { get; }
    IProjectionToken Projection { get; }

    public string GetFullName()
    {
        return Namespace is null ? Name : $"{Namespace}.{Name}";
    }
}
