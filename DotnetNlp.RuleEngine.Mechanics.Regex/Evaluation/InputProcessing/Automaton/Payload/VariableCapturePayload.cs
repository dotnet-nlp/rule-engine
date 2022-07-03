using System.Collections.Generic;
using DotnetNlp.RuleEngine.Core.Evaluation.Cache;
using DotnetNlp.RuleEngine.Core.Evaluation.Rule.Projection.Arguments;
using DotnetNlp.RuleEngine.Mechanics.Regex.Evaluation.InputProcessing.Automaton.Models;
using DotnetNlp.RuleEngine.Mechanics.Regex.Evaluation.InputProcessing.Automaton.Models.States;

namespace DotnetNlp.RuleEngine.Mechanics.Regex.Evaluation.InputProcessing.Automaton.Payload;

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
        string[] sequence,
        RuleSpaceArguments? ruleSpaceArguments,
        RegexAutomatonState targetState,
        AutomatonProgress currentProgress,
        in Stack<AutomatonProgress> progresses,
        IRuleSpaceCache? cache = null
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