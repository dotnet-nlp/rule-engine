using RuleEngine.Mechanics.Regex.Evaluation.InputProcessing.Automaton.Models.States;
using RuleEngine.Mechanics.Regex.Evaluation.InputProcessing.Automaton.Payload;
using RuleEngine.Mechanics.Regex.Evaluation.InputProcessing.Graph;

namespace RuleEngine.Mechanics.Regex.Evaluation.InputProcessing.Automaton.Models.Transitions;

// todo [code quality] separate "build models" (where setters are allowed) and "built models" (which are considered final)
internal sealed class RegexAutomatonTransition : IDigraphEdge<RegexAutomatonState, RegexAutomatonTransition>
{
    public int Id { get; }
    public RegexAutomatonState SourceState { get; private set; }
    public RegexAutomatonState TargetState { get; private set; }
    public ITransitionPayload Payload { get; }
    public IDigraphVertex<RegexAutomatonState, RegexAutomatonTransition> TargetVertex => TargetState;

    public RegexAutomatonTransition(
        int id,
        RegexAutomatonState sourceState,
        RegexAutomatonState targetState,
        ITransitionPayload payload
    )
    {
        Id = id;
        SourceState = sourceState;
        TargetState = targetState;
        Payload = payload;
    }

    public void Reverse()
    {
        var originalSourceState = SourceState;
        var originalTargetState = TargetState;

        ReplaceSourceState(originalTargetState);
        ReplaceTargetState(originalSourceState);
    }

    public void ReplaceSourceState(RegexAutomatonState newSourceState)
    {
        var originalSourceState = SourceState;

        SourceState = newSourceState;

        originalSourceState.OutgoingTransitions.Remove(this);
        SourceState.OutgoingTransitions.Add(this);
    }

    public void ReplaceTargetState(RegexAutomatonState newTargetState)
    {
        var originalTargetState = TargetState;

        TargetState = newTargetState;

        originalTargetState.IncomingTransitions.Remove(this);
        TargetState.IncomingTransitions.Add(this);
    }

    public void Remove()
    {
        SourceState.OutgoingTransitions.Remove(this);
        TargetState.IncomingTransitions.Remove(this);
    }
}