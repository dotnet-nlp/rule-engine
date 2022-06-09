using System.Collections.Generic;
using RuleEngine.Core.Evaluation.Cache;
using RuleEngine.Core.Evaluation.Rule.Input;
using RuleEngine.Mechanics.Regex.Evaluation.InputProcessing.Automaton.Models;
using RuleEngine.Mechanics.Regex.Evaluation.InputProcessing.Automaton.Models.States;
using RuleEngine.Mechanics.Regex.Evaluation.InputProcessing.TerminalDetectors;

namespace RuleEngine.Mechanics.Regex.Evaluation.InputProcessing.Automaton.Payload;

internal sealed class TerminalPayload : ITransitionPayload
{
    public bool IsTransient => false;

    /// <remarks>
    /// Performance remarks: library performance depends on the way this field is declared.
    /// Please make sure you know what you are doing, when changing this field's declaration.
    /// </remarks>
    public readonly ITerminalDetector TerminalDetector;

    public TerminalPayload(ITerminalDetector terminalDetector)
    {
        TerminalDetector = terminalDetector;
    }

    public void Consume(
        RuleInput input,
        RegexAutomatonState targetState,
        AutomatonProgress currentProgress,
        IRuleSpaceCache cache,
        in Stack<AutomatonProgress> progresses
    )
    {
        var nextSymbolIndex = currentProgress.LastUsedSymbolIndex + 1;

        if (nextSymbolIndex < input.Sequence.Length)
        {
            if (TerminalDetector.WordMatches(input.Sequence[nextSymbolIndex], out var explicitlyMatchedSymbolsCount))
            {
                progresses.Push(
                    currentProgress.Clone(
                        targetState,
                        lastUsedSymbolIndex: nextSymbolIndex,
                        explicitlyMatchedSymbolsCount: currentProgress.ExplicitlyMatchedSymbolsCount + explicitlyMatchedSymbolsCount
                    )
                );
            }
        }
    }

    public IEnumerable<string> GetUsedWords()
    {
        return TerminalDetector.GetUsedWords();
    }
}