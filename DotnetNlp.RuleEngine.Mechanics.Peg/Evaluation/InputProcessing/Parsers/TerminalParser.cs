using System;
using System.Collections.Generic;
using DotnetNlp.RuleEngine.Core.Evaluation.Cache;
using DotnetNlp.RuleEngine.Core.Evaluation.Rule.Input;
using DotnetNlp.RuleEngine.Mechanics.Peg.Evaluation.InputProcessing.TerminalDetectors;

namespace DotnetNlp.RuleEngine.Mechanics.Peg.Evaluation.InputProcessing.Parsers;

internal sealed class TerminalParser : IQuantifiableParser
{
    public Type ResultType { get; } = typeof(string);

    private readonly ITerminalDetector _terminalDetector;

    public TerminalParser(ITerminalDetector mTerminalDetector)
    {
        _terminalDetector = mTerminalDetector;
    }

    public bool TryParse(
        RuleInput input,
        IRuleSpaceCache cache,
        ref int index,
        out int explicitlyMatchedSymbolsCount,
        out object? result
    )
    {
        if (index >= input.Sequence.Length)
        {
            explicitlyMatchedSymbolsCount = 0;
            result = null;

            return false;
        }

        var word = input.Sequence[index];

        var isTerminalDetected = _terminalDetector.WordMatches(word);

        if (isTerminalDetected)
        {
            explicitlyMatchedSymbolsCount = 1;
            result = word;

            ++index;
        }
        else
        {
            explicitlyMatchedSymbolsCount = 0;
            result = null;
        }

        return isTerminalDetected;
    }

    public IEnumerable<string> GetUsedWords()
    {
        return _terminalDetector.GetUsedWords();
    }
}