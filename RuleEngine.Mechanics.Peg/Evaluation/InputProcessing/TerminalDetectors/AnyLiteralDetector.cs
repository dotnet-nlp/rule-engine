using System.Collections.Generic;

namespace RuleEngine.Mechanics.Peg.Evaluation.InputProcessing.TerminalDetectors;

internal sealed class AnyLiteralDetector : ITerminalDetector
{
    public static readonly AnyLiteralDetector Instance = new();

    private AnyLiteralDetector()
    {
    }

    public bool WordMatches(string word)
    {
        return true;
    }

    public IEnumerable<string> GetUsedWords()
    {
        yield break;
    }
}