using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using DotnetNlp.RuleEngine.Mechanics.Peg.Build.Tokenization.Tokens;

namespace DotnetNlp.RuleEngine.Mechanics.Peg.Evaluation.InputProcessing.TerminalDetectors;

internal sealed class PrefixDetector : ITerminalDetector
{
    private readonly PrefixToken _prefix;

    public PrefixDetector(PrefixToken prefix)
    {
        _prefix = prefix;
    }

    public bool WordMatches(string word)
    {
        return WordMatches(_prefix, word);
    }

    public IEnumerable<string> GetUsedWords()
    {
        yield break;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool WordMatches(PrefixToken prefix, string word)
    {
        return word.StartsWith(prefix.Prefix, StringComparison.Ordinal);
    }
}