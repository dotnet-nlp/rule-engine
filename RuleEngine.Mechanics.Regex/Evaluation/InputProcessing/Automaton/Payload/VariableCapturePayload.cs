using System.Collections.Generic;
using RuleEngine.Core.Evaluation.Cache;
using RuleEngine.Core.Evaluation.Rule.Input;
using RuleEngine.Mechanics.Regex.Evaluation.InputProcessing.Automaton.Models;
using RuleEngine.Mechanics.Regex.Evaluation.InputProcessing.Automaton.Models.States;

namespace RuleEngine.Mechanics.Regex.Evaluation.InputProcessing.Automaton.Payload;

internal sealed class VariableCapturePayload : ITransitionPayload
{
    public bool IsTransient => true;

    /// <remarks>
    /// Performance remarks: library performance depends on the way this field is declared.
    /// Please make sure you know what you are doing, when changing this field's declaration.
    /// </remarks>
    public readonly string VariableName;

    public VariableCapturePayload(string variableName)
    {
        VariableName = variableName;
    }

    public void Consume(
        RuleInput input,
        RegexAutomatonState targetState,
        AutomatonProgress currentProgress,
        IRuleSpaceCache cache,
        in Stack<AutomatonProgress> progresses
    )
    {
        if (currentProgress.CapturedValueFactory is not null)
        {
            var newProgress = currentProgress.Clone(
                targetState,
                replaceCapturedValueFactory: true,
                capturedValueFactory: null
            );

            newProgress.AddCapturedVariable(
                VariableName,
                currentProgress.CapturedValueFactory()
            );

            progresses.Push(newProgress);
        }
        else
        {
            progresses.Push(currentProgress.Clone(targetState));
        }
    }

    public IEnumerable<string> GetUsedWords()
    {
        yield break;
    }
}