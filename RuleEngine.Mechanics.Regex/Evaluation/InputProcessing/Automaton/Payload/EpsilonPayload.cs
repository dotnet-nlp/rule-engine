using System.Collections.Generic;
using RuleEngine.Core.Evaluation.Cache;
using RuleEngine.Core.Evaluation.Rule.Input;
using RuleEngine.Mechanics.Regex.Evaluation.InputProcessing.Automaton.Models;
using RuleEngine.Mechanics.Regex.Evaluation.InputProcessing.Automaton.Models.States;

namespace RuleEngine.Mechanics.Regex.Evaluation.InputProcessing.Automaton.Payload;

internal sealed class EpsilonPayload : ITransitionPayload
{
    public static readonly EpsilonPayload Instance = new();

    public bool IsTransient => true;

    private EpsilonPayload()
    {
    }

    public void Consume(
        RuleInput input,
        RegexAutomatonState targetState,
        AutomatonProgress currentProgress,
        IRuleSpaceCache cache,
        in Stack<AutomatonProgress> progresses
    )
    {
        progresses.Push(currentProgress.Clone(targetState));
    }

    public IEnumerable<string> GetUsedWords()
    {
        yield break;
    }
}