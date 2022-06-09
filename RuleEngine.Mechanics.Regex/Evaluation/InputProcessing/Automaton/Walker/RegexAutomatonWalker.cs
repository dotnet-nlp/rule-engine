using System.Collections.Generic;
using RuleEngine.Core.Evaluation.Cache;
using RuleEngine.Core.Evaluation.Rule.Input;
using RuleEngine.Core.Evaluation.Rule.Result;
using RuleEngine.Mechanics.Regex.Evaluation.InputProcessing.Automaton.Models;

namespace RuleEngine.Mechanics.Regex.Evaluation.InputProcessing.Automaton.Walker;

internal sealed class RegexAutomatonWalker : IRegexAutomatonWalker<RegexAutomaton>
{
    public static readonly RegexAutomatonWalker Instance = new();

    public RuleMatchResultCollection Walk(
        RegexAutomaton automaton,
        RuleInput ruleInput,
        int firstSymbolIndex,
        IRuleSpaceCache cache
    )
    {
        // todo this is rough estimate, think if we can predict this number more precisely
        var results = new RuleMatchResultCollection(10);
        var progresses = new Stack<AutomatonProgress>(10);

        progresses.Push(
            new AutomatonProgress(
                firstSymbolIndex - 1,
                null,
                0,
                null,
                null,
                automaton.StartState
            )
        );

        while (progresses.TryPop(out var progress))
        {
            if (progress.State.Id == automaton.EndState.Id)
            {
                results.Add(progress.Flush(ruleInput, firstSymbolIndex));
                continue;
            }

            foreach (var transition in progress.State.OutgoingTransitions)
            {
                transition.Payload.Consume(ruleInput, transition.TargetState, progress, cache, progresses);
            }
        }

        return results;
    }
}