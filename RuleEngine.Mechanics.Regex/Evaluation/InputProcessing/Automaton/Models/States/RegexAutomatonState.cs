using System.Collections.Generic;
using RuleEngine.Mechanics.Regex.Evaluation.InputProcessing.Automaton.Models.Transitions;
using RuleEngine.Mechanics.Regex.Evaluation.InputProcessing.Graph;
using RuleEngine.Mechanics.Regex.Exceptions;

namespace RuleEngine.Mechanics.Regex.Evaluation.InputProcessing.Automaton.Models.States;

// todo [code quality] separate "build models" (where setters are allowed) and "built models" (which are considered final)
// todo [realtime performance] make OutgoingTransitions declared as RegexAutomatonTransition[] (list slows things down)
internal sealed class RegexAutomatonState : IDigraphVertex<RegexAutomatonState, RegexAutomatonTransition>
{
    public int Id { get; }
    public List<RegexAutomatonTransition> IncomingTransitions { get; }
    public List<RegexAutomatonTransition> OutgoingTransitions { get; }

    public IReadOnlyCollection<IDigraphEdge<RegexAutomatonState, RegexAutomatonTransition>> Edges => OutgoingTransitions;

    public RegexAutomatonState(int id, int outgoingTransitionsCount, int incomingTransitionsCount)
    {
        Id = id;
        // todo [realtime performance] we can try to initialize this collections in a lazy way
        OutgoingTransitions = new List<RegexAutomatonTransition>(outgoingTransitionsCount);
        IncomingTransitions = new List<RegexAutomatonTransition>(incomingTransitionsCount);
    }

    public void MoveIncomingTransitionsToOtherState(RegexAutomatonState other)
    {
        // todo [realtime performance] we can try perform replacement in some other way to remove this ToArray call
        foreach (var incomingTransition in IncomingTransitions.ToArray())
        {
            incomingTransition.ReplaceTargetState(other);
        }

        if (IncomingTransitions.Count > 0)
        {
            throw new RegexProcessorBuildException(
                $"Cannot move incoming transitions from state {Id} to state {other.Id}: " +
                $"incoming transitions list is not empty."
            );
        }
    }

    public void MoveOutgoingTransitionsToOtherState(RegexAutomatonState other)
    {
        // todo [realtime performance] we can try perform replacement in some other way to remove this ToArray call
        foreach (var outgoingTransition in OutgoingTransitions.ToArray())
        {
            outgoingTransition.ReplaceSourceState(other);
        }

        if (OutgoingTransitions.Count > 0)
        {
            throw new RegexProcessorBuildException(
                $"Cannot move outgoing transitions from state {Id} to state {other.Id}: " +
                $"outgoing transitions list is not empty."
            );
        }
    }
}