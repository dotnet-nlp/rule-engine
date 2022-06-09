using System.Linq;
using IronMeta.Matcher;
using RuleEngine.Core.Build.Tokenization;
using RuleEngine.Core.Build.Tokenization.Tokens;
using RuleEngine.Core.Lib.CodeAnalysis.Tokenization.Grammar;
using RuleEngine.Core.Lib.CodeAnalysis.Tokenization.Tokens;
using RuleEngine.Mechanics.Peg.Build.Tokenization.Grammar;
using RuleEngine.Mechanics.Peg.Exceptions;

namespace RuleEngine.Mechanics.Peg.Build.Tokenization;

public sealed class GrammarBasedPegPatternTokenizer : IPatternTokenizer
{
    public IPatternToken Tokenize(string pattern, string? @namespace, bool caseSensitive)
    {
        if (!caseSensitive)
        {
            throw new PegPatternTokenizationException(
                $"Case insensitive PEG tokenization is not supported by {nameof(GrammarBasedPegPatternTokenizer)}.",
                pattern
            );
        }

        var matcher = new PegSyntaxMatcher(@namespace, new CSharpSyntaxMatcher());
        MatchResult<char, IToken> result = matcher.GetMatch(pattern.ReplaceLineEndings().ToList(), matcher.Pattern);

        if (!result.Success)
        {
            throw new PegPatternTokenizationException("Failed to parse PEG pattern.", pattern);
        }

        if (result.Result is null)
        {
            throw new PegPatternTokenizationException("Root token is null.", pattern);
        }

        if (result.Result is not IPatternToken patternToken)
        {
            throw new PegPatternTokenizationException($"Root token is of unexpected type '{result.Result.GetType().Name}'.", pattern);
        }

        return patternToken;
    }
}