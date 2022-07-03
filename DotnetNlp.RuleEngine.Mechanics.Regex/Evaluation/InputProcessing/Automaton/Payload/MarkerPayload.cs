using System.Collections.Generic;
using DotnetNlp.RuleEngine.Core.Evaluation.Cache;
using DotnetNlp.RuleEngine.Core.Evaluation.Rule.Input;
using DotnetNlp.RuleEngine.Mechanics.Regex.Evaluation.InputProcessing.Automaton.Models;
using DotnetNlp.RuleEngine.Mechanics.Regex.Evaluation.InputProcessing.Automaton.Models.States;

namespace DotnetNlp.RuleEngine.Mechanics.Regex.Evaluation.InputProcessing.Automaton.Payload;

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
        in Stack<AutomatonProgress> progresses,
        IRuleSpaceCache? cache = null
    )
    {
        progresses.Push(currentProgress.Clone(targetState, replaceMarker: true, marker: Marker));
    }

    public IEnumerable<string> GetUsedWords()
    {
        yield break;
    }
}