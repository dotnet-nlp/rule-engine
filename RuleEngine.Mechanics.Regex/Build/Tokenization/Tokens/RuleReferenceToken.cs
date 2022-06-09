using System.Linq;
using RuleEngine.Core.Build.Tokenization.Tokens;
using RuleEngine.Core.Build.Tokenization.Tokens.Arguments;
using RuleEngine.Core.Lib.Common.Helpers;

namespace RuleEngine.Mechanics.Regex.Build.Tokenization.Tokens;

public sealed class RuleReferenceToken : IQuantifiableToken, IRuleReferenceToken
{
    public string? DeclaredInNamespace { get; }
    public string RuleName { get; }
    public IRuleArgumentToken[] Arguments { get; }

    public RuleReferenceToken(string? declaredInNamespace, string ruleName, IRuleArgumentToken[] arguments)
    {
        DeclaredInNamespace = declaredInNamespace;
        RuleName = ruleName;
        Arguments = arguments;
    }

    public override string ToString()
    {
        return $"<{RuleName}{(Arguments.Length == 0 ? string.Empty : $"({Arguments.Select(argument => argument.ToString()).JoinToString(", ")})")}>";
    }
}