using System.Collections.Generic;
using RuleEngine.Core.Evaluation.Cache;
using RuleEngine.Core.Evaluation.Rule.Input;
using RuleEngine.Mechanics.Regex.Evaluation.InputProcessing.Automaton.Models;
using RuleEngine.Mechanics.Regex.Evaluation.InputProcessing.Automaton.Models.States;

namespace RuleEngine.Mechanics.Regex.Evaluation.InputProcessing.Automaton.Payload;

internal sealed class MarkerPayload : ITransitionPayload
{
    public bool IsTransient => true;

    /// <remarks>
    /// Performance remarks: library performance depends on the way this field is declared.
    /// Please make sure you know what you are doing, when changing this field's declaration.
    /// </remarks>
    public readonly string Marker;

    public MarkerPayload(string marker)
    {
        Marker = marker;
    }

    public void Consume(
        RuleInput input,
        RegexAutomatonState targetState,
        AutomatonProgress currentProgress,
        IRuleSpaceCache cache,
        in Stack<AutomatonProgress> progresses
    )
    {
        progresses.Push(currentProgress.Clone(targetState, replaceMarker: true, marker: Marker));
    }

    public IEnumerable<string> GetUsedWords()
    {
        yield break;
    }
}