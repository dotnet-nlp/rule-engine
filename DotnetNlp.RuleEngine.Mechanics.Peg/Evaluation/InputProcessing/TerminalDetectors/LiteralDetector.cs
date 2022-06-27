using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using DotnetNlp.RuleEngine.Mechanics.Peg.Build.Tokenization.Tokens;

namespace DotnetNlp.RuleEngine.Mechanics.Peg.Evaluation.InputProcessing.TerminalDetectors;

internal sealed class LiteralDetector : ITerminalDetector
{
    private readonly LiteralToken _literal;

    public LiteralDetector(LiteralToken literal)
    {
        _literal = literal;
    }

    public bool WordMatches(string word)
    {
        return WordMatches(_literal, word);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool WordMatches(LiteralToken literal, string word)
    {
        return word.Equals(literal.Literal, StringComparison.Ordinal);
    }

    public IEnumerable<string> GetUsedWords()
    {
        yield return _literal.Literal;
    }
}