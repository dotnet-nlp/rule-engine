using DotnetNlp.RuleEngine.Core.Build.Tokenization.Tokens;

namespace DotnetNlp.RuleEngine.Core.Build.Tokenization;

public interface IRuleSetTokenizer
{
    RuleSetToken Tokenize(string ruleSet, string? @namespace, bool caseSensitive);
}