using RuleEngine.Core.Build.Tokenization.Tokens;

namespace RuleEngine.Core.Build.Tokenization;

public interface IPatternTokenizer
{
    IPatternToken Tokenize(string pattern, string? @namespace, bool caseSensitive);
}