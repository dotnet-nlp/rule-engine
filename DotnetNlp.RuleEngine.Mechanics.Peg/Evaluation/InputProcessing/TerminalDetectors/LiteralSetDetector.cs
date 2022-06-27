using System.Collections.Generic;
using DotnetNlp.RuleEngine.Mechanics.Peg.Build.Tokenization.Tokens;
using DotnetNlp.RuleEngine.Mechanics.Peg.Exceptions;

namespace DotnetNlp.RuleEngine.Mechanics.Peg.Evaluation.InputProcessing.TerminalDetectors;

internal sealed class LiteralSetDetector : ITerminalDetector
{
    private readonly LiteralSetToken _token;

    public LiteralSetDetector(LiteralSetToken token)
    {
        _token = token;
    }

    public bool WordMatches(string word)
    {
        var hasPositiveMatch = false;

        foreach (var member in _token.Members)
        {
            var memberMatched = member switch
            {
                LiteralToken literalToken => LiteralDetector.WordMatches(literalToken, word),
                PrefixToken prefixToken => PrefixDetector.WordMatches(prefixToken, word),
                InfixToken infixToken => InfixDetector.WordMatches(infixToken, word),
                SuffixToken suffixToken => SuffixDetector.WordMatches(suffixToken, word),
                _ => throw new PegProcessorMatchException($"Unknown literal set member type {member.GetType().FullName}."),
            };

            if (memberMatched)
            {
                hasPositiveMatch = true;
                break;
            }
        }

        return _token.IsNegative != hasPositiveMatch;
    }

    public IEnumerable<string> GetUsedWords()
    {
        foreach (var member in _token.Members)
        {
            if (member is LiteralToken literalToken)
            {
                yield return literalToken.Literal;
            }
        }
    }
}