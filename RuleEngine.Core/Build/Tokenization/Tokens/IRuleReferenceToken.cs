using RuleEngine.Core.Build.Tokenization.Tokens.Arguments;

namespace RuleEngine.Core.Build.Tokenization.Tokens;

public interface IRuleReferenceToken
{
    string RuleName { get; }
    string? DeclaredInNamespace { get; }
    IRuleArgumentToken[] Arguments { get; }

    public string GetRuleSpaceKey()
    {
        // if reference is fully-qualified
        if (RuleName.Contains('.'))
        {
            return RuleName;
        }

        // if we can't add namespace
        if (DeclaredInNamespace is null)
        {
            return RuleName;
        }

        return $"{DeclaredInNamespace}.{RuleName}";
    }
}