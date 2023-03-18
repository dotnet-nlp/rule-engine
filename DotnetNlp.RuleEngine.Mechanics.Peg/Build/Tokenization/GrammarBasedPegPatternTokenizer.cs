using System.Linq;
using DotnetNlp.RuleEngine.Core.Build.Tokenization;
using DotnetNlp.RuleEngine.Core.Build.Tokenization.Tokens;
using DotnetNlp.RuleEngine.Core.Lib.CodeAnalysis.Tokenization.Grammar;
using DotnetNlp.RuleEngine.Mechanics.Peg.Build.Tokenization.Grammar;
using DotnetNlp.RuleEngine.Mechanics.Peg.Exceptions;

namespace DotnetNlp.RuleEngine.Mechanics.Peg.Build.Tokenization;

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
        var result = matcher.GetMatch(pattern.ReplaceLineEndings().ToList(), matcher.Pattern);

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