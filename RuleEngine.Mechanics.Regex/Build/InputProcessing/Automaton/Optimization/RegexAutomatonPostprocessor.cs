using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using RuleEngine.Core.Build;
using RuleEngine.Mechanics.Regex.Build.InputProcessing.Automaton.Equality;
using RuleEngine.Mechanics.Regex.Evaluation.InputProcessing.Automaton.Models;
using RuleEngine.Mechanics.Regex.Evaluation.InputProcessing.Automaton.Models.States;
using RuleEngine.Mechanics.Regex.Evaluation.InputProcessing.Automaton.Models.Transitions;
using RuleEngine.Mechanics.Regex.Evaluation.InputProcessing.Automaton.Payload;
using RuleEngine.Mechanics.Regex.Evaluation.InputProcessing.Graph.Walker;
using RuleEngine.Mechanics.Regex.Exceptions;

namespace RuleEngine.Mechanics.Regex.Build.InputProcessing.Automaton.Optimization;

internal sealed class RegexAutomatonPostprocessor
{
    public static readonly RegexAutomatonPostprocessor Instance = new(RecursiveDfsDigraphWalker.Instance);

    private readonly IDigraphWalker _digraphWalker;

    private RegexAutomatonPostprocessor(IDigraphWalker digraphWalker)
    {
        _digraphWalker = digraphWalker;
    }

    public void ValidateAndOptimize(
        RegexAutomaton automaton,
        OptimizationLevel optimizationLevel,
        IRuleSpaceDescription ruleSpaceDescription
    )
    {
        RunAllOptimizationsAndValidation(automaton, new HashSet<int>(), ruleSpaceDescription);

        if (optimizationLevel == OptimizationLevel.Min)
        {
            return;
        }

        // todo [non-realtime performance] we can avoid reversing automaton back and forward
        // and just replace Incoming/Outgoing, Source/Target, and Start/End values in the code that does the optimization
        // (i.e. we simulate reversing by using reversed properties)
        automaton.Reverse(_digraphWalker);

        RunAllOptimizationsAndValidation(automaton, null, null);

        automaton.Reverse(_digraphWalker);
    }

    private static void RunAllOptimizationsAndValidation(
        RegexAutomaton automaton,
        ISet<int>? localEpsilonClosure,
        IRuleSpaceDescription? ruleSpaceDescription
    )
    {
        ISet<int> visitedStates = new HashSet<int>();

        Walk(automaton.StartState);

        void Walk(RegexAutomatonState state)
        {
            if (visitedStates.Add(state.Id))
            {
                if (state.Id != automaton.EndState.Id)
                {
                    if (TryEliminateState(state, out var newState))
                    {
                        Walk(newState);
                    }
                    else
                    {
                        if (!TryMergeSiblings(state, out var transitionsToFollow))
                        {
                            transitionsToFollow = state.OutgoingTransitions;
                        }

                        if (localEpsilonClosure is not null)
                        {
                            DiscoverEpsilonClosure(transitionsToFollow, state.Id);
                            localEpsilonClosure.Clear();
                        }

                        foreach (var transitionToFollow in transitionsToFollow)
                        {
                            if (ruleSpaceDescription is not null &&
                                transitionToFollow.Payload is IRuleReferencePayload ruleReferencePayload
                               )
                            {
                                ruleSpaceDescription.ThrowIfNotExists(ruleReferencePayload.RuleSpaceKey);
                            }

                            Walk(transitionToFollow.TargetState);
                        }
                    }
                }
            }
        }

        void DiscoverEpsilonClosure(IEnumerable<RegexAutomatonTransition> transitions, int referenceStateId)
        {
            foreach (var transition in transitions.Where(transition => transition.Payload.IsTransient))
            {
                if (!localEpsilonClosure.Add(transition.Id))
                {
                    throw new RegexProcessorBuildException($"Found undeterministic loop on state '{referenceStateId}'.");
                }

                DiscoverEpsilonClosure(transition.TargetState.OutgoingTransitions, transition.TargetState.Id);
            }
        }

        bool TryEliminateState(
            RegexAutomatonState state,
            [MaybeNullWhen(false)] out RegexAutomatonState replacement
        )
        {
            if (state.Id != automaton.StartState.Id && state.OutgoingTransitions.Count == 1)
            {
                var outgoingTransition = state.OutgoingTransitions[0];

                if (outgoingTransition.Payload is EpsilonPayload)
                {
                    state.MoveIncomingTransitionsToOtherState(outgoingTransition.TargetState);

                    outgoingTransition.Remove();

                    replacement = outgoingTransition.TargetState;
                    return true;
                }
            }

            replacement = null;

            return false;
        }

        bool TryMergeSiblings(
            RegexAutomatonState state,
            [MaybeNullWhen(false)] out List<RegexAutomatonTransition> transitionsToFollow
        )
        {
            if (state.OutgoingTransitions.Count > 1)
            {
                var newTransitions = new List<RegexAutomatonTransition>(
                    state.OutgoingTransitions.Count
                );

                var groupingsByPayload = state
                    .OutgoingTransitions
                    .GroupBy(transition => transition.Payload, TransitionPayloadEqualityComparer.Instance);

                foreach (var payloadGrouping in groupingsByPayload)
                {
                    var groupingsByTargetState = payloadGrouping
                        .GroupBy(transition => transition.TargetState.Id)
                        .ToArray();

                    var mergeableGroupings = groupingsByTargetState
                        .Where(targetStateGrouping => targetStateGrouping.Count() > 1)
                        .ToArray();

                    var orphans = groupingsByTargetState
                        .Except(mergeableGroupings)
                        .SelectMany(targetStateGrouping => targetStateGrouping);

                    MergeTransitions(orphans, newTransitions, false);

                    foreach (var mergeableGrouping in mergeableGroupings)
                    {
                        MergeTransitions(mergeableGrouping, newTransitions, true);
                    }
                }

                if (newTransitions.Count == 1 && TryEliminateState(newTransitions[0].SourceState, out var newSourceState))
                {
                    transitionsToFollow = newSourceState.OutgoingTransitions;
                }
                else
                {
                    transitionsToFollow = newTransitions;
                }

                return true;
            }

            transitionsToFollow = null;

            return false;
        }
    }

    private static void MergeTransitions(
        IEnumerable<RegexAutomatonTransition> transitions,
        in List<RegexAutomatonTransition> newTransitions,
        bool hasSameTargetState
    )
    {
        RegexAutomatonTransition? mainOutgoingTransition = null;
        foreach (var transition in transitions)
        {
            if (!hasSameTargetState && transition.TargetState.IncomingTransitions.Count != 1)
            {
                newTransitions.Add(transition);
                continue;
            }

            if (mainOutgoingTransition is null)
            {
                mainOutgoingTransition = transition;
                newTransitions.Add(transition);
                continue;
            }

            var duplicateOutgoingTransition = transition;

            if (!hasSameTargetState)
            {
                duplicateOutgoingTransition.TargetState.MoveOutgoingTransitionsToOtherState(mainOutgoingTransition.TargetState);
            }

            duplicateOutgoingTransition.Remove();
        }
    }
}