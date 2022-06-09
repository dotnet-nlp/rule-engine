using System.Collections.Generic;
using System.Runtime.CompilerServices;
using RuleEngine.Mechanics.Peg.Build.Tokenization.Tokens;

namespace RuleEngine.Mechanics.Peg.Evaluation.InputProcessing.TerminalDetectors;

internal sealed class InfixDetector : ITerminalDetector
{
    private readonly InfixToken _infix;

    public InfixDetector(InfixToken infix)
    {
        _infix = infix;
    }

    public bool WordMatches(string word)
    {
        return WordMatches(_infix, word);
    }

    public IEnumerable<string> GetUsedWords()
    {
        yield break;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool WordMatches(InfixToken infix, string word)
    {
        return word.Contains(infix.Infix, System.StringComparison.Ordinal);
    }
}