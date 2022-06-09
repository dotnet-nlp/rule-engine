using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using RuleEngine.Mechanics.Peg.Build.Tokenization.Tokens;

namespace RuleEngine.Mechanics.Peg.Evaluation.InputProcessing.TerminalDetectors;

internal sealed class SuffixDetector : ITerminalDetector
{
    private readonly SuffixToken _suffix;

    public SuffixDetector(SuffixToken suffix)
    {
        _suffix = suffix;
    }

    public bool WordMatches(string word)
    {
        return WordMatches(_suffix, word);
    }

    public IEnumerable<string> GetUsedWords()
    {
        yield break;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool WordMatches(SuffixToken suffix, string word)
    {
        return word.EndsWith(suffix.Suffix, StringComparison.Ordinal);
    }
}