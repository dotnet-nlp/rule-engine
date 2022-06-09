using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using RuleEngine.Mechanics.Regex.Build.Tokenization.Tokens;

namespace RuleEngine.Mechanics.Regex.Evaluation.InputProcessing.TerminalDetectors;

internal sealed class SuffixDetector : ITerminalDetector
{
    public readonly string Suffix;

    public SuffixDetector(SuffixToken suffix)
    {
        Suffix = suffix.Suffix;
    }

    public bool WordMatches(string word, out int explicitlyMatchedSymbolsCount)
    {
        if (WordMatches(Suffix, word))
        {
            explicitlyMatchedSymbolsCount = 1;
            return true;
        }

        explicitlyMatchedSymbolsCount = 0;
        return false;
    }

    public IEnumerable<string> GetUsedWords()
    {
        yield break;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool WordMatches(string suffix, string word)
    {
        return word.EndsWith(suffix, StringComparison.Ordinal);
    }
}