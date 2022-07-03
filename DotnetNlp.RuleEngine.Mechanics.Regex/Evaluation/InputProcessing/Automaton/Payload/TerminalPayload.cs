using System.Collections.Generic;
using DotnetNlp.RuleEngine.Core.Evaluation.Cache;
using DotnetNlp.RuleEngine.Core.Evaluation.Rule.Input;
using DotnetNlp.RuleEngine.Mechanics.Regex.Evaluation.InputProcessing.Automaton.Models;
using DotnetNlp.RuleEngine.Mechanics.Regex.Evaluation.InputProcessing.Automaton.Models.States;
using DotnetNlp.RuleEngine.Mechanics.Regex.Evaluation.InputProcessing.TerminalDetectors;

namespace DotnetNlp.RuleEngine.Mechanics.Regex.Evaluation.InputProcessing.Automaton.Payload;

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
        in Stack<AutomatonProgress> progresses,
        IRuleSpaceCache? cache = null
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