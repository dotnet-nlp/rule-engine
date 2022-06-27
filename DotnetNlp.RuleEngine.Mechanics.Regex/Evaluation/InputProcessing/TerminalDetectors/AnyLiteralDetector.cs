using System.Collections.Generic;

namespace DotnetNlp.RuleEngine.Mechanics.Regex.Evaluation.InputProcessing.TerminalDetectors;

internal sealed class AnyLiteralDetector : ITerminalDetector
{
    public static readonly AnyLiteralDetector Instance = new();

    private AnyLiteralDetector()
    {
    }

    public bool WordMatches(string word, out int explicitlyMatchedSymbolsCount)
    {
        explicitlyMatchedSymbolsCount = 0;

        return true;
    }

    public IEnumerable<string> GetUsedWords()
    {
        yield break;
    }
}