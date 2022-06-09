using System.Collections.Generic;
using System.Runtime.CompilerServices;
using RuleEngine.Mechanics.Regex.Build.Tokenization.Tokens;

namespace RuleEngine.Mechanics.Regex.Evaluation.InputProcessing.TerminalDetectors;

internal sealed class InfixDetector : ITerminalDetector
{
    /// <remarks>
    /// Performance remarks: library performance depends on the way this field is declared.
    /// Please make sure you know what you are doing, when changing this field's declaration.
    /// </remarks>
    public readonly string Infix;

    public InfixDetector(InfixToken infix)
    {
        Infix = infix.Infix;
    }

    public bool WordMatches(string word, out int explicitlyMatchedSymbolsCount)
    {
        if (WordMatches(Infix, word))
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
    public static bool WordMatches(string infix, string word)
    {
        return word.Contains(infix, System.StringComparison.Ordinal);
    }
}