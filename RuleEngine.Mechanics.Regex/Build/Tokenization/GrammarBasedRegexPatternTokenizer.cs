using System.Linq;
using RuleEngine.Mechanics.Regex.Build.Tokenization.Grammar;
using RuleEngine.Mechanics.Regex.Exceptions;
using IronMeta.Matcher;
using RuleEngine.Core.Build.Tokenization;
using RuleEngine.Core.Build.Tokenization.Tokens;
using RuleEngine.Core.Lib.CodeAnalysis.Tokenization.Grammar;
using IToken = RuleEngine.Core.Lib.CodeAnalysis.Tokenization.Tokens.IToken;

namespace RuleEngine.Mechanics.Regex.Build.Tokenization;

public sealed class GrammarBasedRegexPatternTokenizer : IPatternTokenizer
{
    public IPatternToken Tokenize(string pattern, string? @namespace, bool caseSensitive)
    {
        pattern = pattern.ReplaceLineEndings();

        if (!caseSensitive)
        {
            throw new RegexPatternTokenizationException(
                $"Case insensitive PEG tokenization is not supported by {nameof(GrammarBasedRegexPatternTokenizer)}.",
                pattern
            );
        }

        var matcher = new RegexSyntaxMatcher(@namespace, new CSharpSyntaxMatcher());
        MatchResult<char, IToken> result = matcher.GetMatch(pattern.ReplaceLineEndings().ToList(), matcher.Pattern);

        if (!result.Success)
        {
            throw new RegexPatternTokenizationException("Failed to parse regex pattern.", pattern);
        }

        if (result.Result is null)
        {
            throw new RegexPatternTokenizationException("Root token is null.", pattern);
        }

        if (result.Result is not IPatternToken patternToken)
        {
            throw new RegexPatternTokenizationException($"Root token is of unexpected type '{result.Result.GetType().Name}'.", pattern);
        }

        return patternToken;
    }
}