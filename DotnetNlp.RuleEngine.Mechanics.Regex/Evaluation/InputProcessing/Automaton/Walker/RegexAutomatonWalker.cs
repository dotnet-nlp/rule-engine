using System.Collections.Generic;
using DotnetNlp.RuleEngine.Core.Evaluation.Cache;
using DotnetNlp.RuleEngine.Core.Evaluation.Rule.Projection.Arguments;
using DotnetNlp.RuleEngine.Core.Evaluation.Rule.Result;
using DotnetNlp.RuleEngine.Mechanics.Regex.Evaluation.InputProcessing.Automaton.Models;

namespace DotnetNlp.RuleEngine.Mechanics.Regex.Evaluation.InputProcessing.Automaton.Walker;

internal sealed class RegexAutomatonWalker : IRegexAutomatonWalker<RegexAutomaton>
{
    public static readonly RegexAutomatonWalker Instance = new();

    public RuleMatchResultCollection Walk(
        RegexAutomaton automaton,
        string[] sequence,
        int firstSymbolIndex = 0,
        // todo [code quality] make a wrapper over RegexAutomaton to completely separate automaton walker from rule space
        RuleSpaceArguments? ruleSpaceArguments = null,
        IRuleSpaceCache? cache = null
    )
    {
        // todo think if we can predict this number more precisely
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
                results.Add(progress.Flush(sequence, firstSymbolIndex));
                continue;
            }

            foreach (var transition in progress.State.OutgoingTransitions)
            {
                transition.Payload.Consume(sequence, ruleSpaceArguments, transition.TargetState, progress, progresses, cache);
            }
        }

        return results;
    }
}