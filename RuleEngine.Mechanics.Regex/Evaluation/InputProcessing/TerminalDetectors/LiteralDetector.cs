using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using RuleEngine.Mechanics.Regex.Build.Tokenization.Tokens;

namespace RuleEngine.Mechanics.Regex.Evaluation.InputProcessing.TerminalDetectors;

internal sealed class LiteralDetector : ITerminalDetector
{
    /// <remarks>
    /// Performance remarks: library performance depends on the way this field is declared.
    /// Please make sure you know what you are doing, when changing this field's declaration.
    /// </remarks>
    public readonly string Literal;

    public LiteralDetector(LiteralToken literal)
    {
        Literal = literal.Literal;
    }

    public bool WordMatches(string word, out int explicitlyMatchedSymbolsCount)
    {
        if (WordMatches(Literal, word))
        {
            explicitlyMatchedSymbolsCount = 1;
            return true;
        }

        explicitlyMatchedSymbolsCount = 0;
        return false;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool WordMatches(string literal, string word)
    {
        return word.Equals(literal, StringComparison.Ordinal);
    }

    public IEnumerable<string> GetUsedWords()
    {
        yield return Literal;
    }
}