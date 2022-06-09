using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using RuleEngine.Mechanics.Regex.Build.Tokenization.Tokens;

namespace RuleEngine.Mechanics.Regex.Evaluation.InputProcessing.TerminalDetectors;

internal sealed class PrefixDetector : ITerminalDetector
{
    public readonly string Prefix;

    public PrefixDetector(PrefixToken prefix)
    {
        Prefix = prefix.Prefix;
    }

    public bool WordMatches(string word, out int explicitlyMatchedSymbolsCount)
    {
        if (WordMatches(Prefix, word))
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
    public static bool WordMatches(string prefix, string word)
    {
        return word.StartsWith(prefix, StringComparison.Ordinal);
    }
}