using System;
using System.Collections.Generic;
using DotnetNlp.RuleEngine.Core.Build.Tokenization.Tokens;
using DotnetNlp.RuleEngine.Core.Evaluation;
using DotnetNlp.RuleEngine.Mechanics.Regex.Build.Tokenization.Tokens;
using DotnetNlp.RuleEngine.Mechanics.Regex.Evaluation.InputProcessing.Automaton.Models;
using DotnetNlp.RuleEngine.Mechanics.Regex.Evaluation.InputProcessing.Automaton.Models.States;
using DotnetNlp.RuleEngine.Mechanics.Regex.Evaluation.InputProcessing.Automaton.Models.Transitions;
using DotnetNlp.RuleEngine.Mechanics.Regex.Evaluation.InputProcessing.Automaton.Payload;
using DotnetNlp.RuleEngine.Mechanics.Regex.Evaluation.InputProcessing.TerminalDetectors;
using DotnetNlp.RuleEngine.Mechanics.Regex.Exceptions;

namespace DotnetNlp.RuleEngine.Mechanics.Regex.Build.InputProcessing.Automaton;

/// <summary>
/// The logic of this builder is built on top of the Thompson's construction methods.
/// It is, though, highly optimized: it doesn't create any extra epsilon-transitions,
/// it tries to predict the count of incoming/outgoing transitions for each state (to minimize List resizing>)
/// And, finally, it uses a different logic of quantification: Thompson's construction assumes
/// that a{0, 2} == a? a?, and logic of this class uses a separate automaton structure,
/// which minimizes the amount of progresses, that will be produced after passing a{0, 2} block.
/// </summary>
/// <remarks>
/// Instance of this class holds some state: next ids for new states and transitions.
/// This means, if the goal is to build independent automatons, the instance of this factory
/// should be created per each such operation. Otherwise, the same instance should be reused.
/// </remarks>
internal sealed class RegexAutomatonBuilder
{
    private readonly RegexGroupToken _groupToken;
    private readonly IRuleSpace _ruleSpace;

    private int _nextStateId = 0;
    private int NextStateId => _nextStateId++;

    private int _nextTransitionId = 0;
    private int NextTransitionId => _nextTransitionId++;

    public RegexAutomatonBuilder(RegexGroupToken groupToken, IRuleSpace ruleSpace)
    {
        _groupToken = groupToken;
        _ruleSpace = ruleSpace;
    }

    public (RegexAutomaton Automaton, IReadOnlySet<string> Dependencies) Build()
    {
        var dependencies = new HashSet<string>();
        var automaton = BuildGroupAutomaton(_groupToken, dependencies);

        return (automaton, dependencies);
    }

    private RegexAutomaton BuildGroupAutomaton(
        RegexGroupToken groupToken,
        HashSet<string> dependencies,
        (RegexAutomatonState StartState, int EndStateOutgoingTransitionsCount, int AdditionalEndStateIncomingTransitionsCount)? parent = null
    )
    {

        var groupAutomaton = new RegexAutomaton(
            parent?.StartState ?? new RegexAutomatonState(NextStateId, groupToken.Branches.Length, 0),
            new RegexAutomatonState(
                NextStateId,
                parent?.EndStateOutgoingTransitionsCount ?? 1,
                groupToken.Branches.Length + (parent?.AdditionalEndStateIncomingTransitionsCount ?? 0)
            )
        );

        foreach (var branchToken in groupToken.Branches)
        {
            var previousState = groupAutomaton.StartState;
            IBranchItemToken? currentBranchItem = null;

            for (var nextIndex = 0; nextIndex <= branchToken.Items.Length; nextIndex++)
            {
                var isIndexInRange = nextIndex < branchToken.Items.Length;

                if (currentBranchItem is null)
                {
                    if (!isIndexInRange)
                    {
                        break;
                    }

                    currentBranchItem = branchToken.Items[nextIndex];
                    continue;
                }

                var nextBranchItem = isIndexInRange ? branchToken.Items[nextIndex] : null;

                var endStateOutgoingTransitionsCount = nextBranchItem is not null
                    ? GetOutgoingTransitionsCount(nextBranchItem)
                    : parent?.EndStateOutgoingTransitionsCount ?? 1;

                previousState = currentBranchItem switch
                {
                    MarkerToken marker => BuildMarkerAutomaton(
                        marker,
                        previousState,
                        endStateOutgoingTransitionsCount
                    ),
                    QuantifiableBranchItemToken quantifiable => BuildQuantifiableAutomaton(
                        quantifiable,
                        previousState,
                        endStateOutgoingTransitionsCount,
                        dependencies
                    ),
                    _ => throw new RegexProcessorBuildException($"Unknown branch item type {currentBranchItem.GetType().FullName}."),
                };

                currentBranchItem = nextBranchItem;
            }

            previousState.MoveIncomingTransitionsToOtherState(groupAutomaton.EndState);
        }

        return groupAutomaton;
    }

    private static int GetOutgoingTransitionsCount(IBranchItemToken branchItem)
    {
        return branchItem switch
        {
            MarkerToken => 1,
            QuantifiableBranchItemToken token => FromQuantifiable(token.Quantifiable) + FromQuantifier(token.Quantifier),
            _ => throw new ArgumentOutOfRangeException(nameof(branchItem)),
        };

        int FromQuantifiable(IQuantifiableToken quantifiable)
        {
            return quantifiable switch
            {
                ITerminalToken => 1,
                RegexGroupToken group => group.Branches.Length,
                RuleReferenceToken => 1,
                _ => throw new RegexProcessorBuildException(
                    $"Unknown quantifiable token type '{quantifiable.GetType().FullName}'."
                ),
            };
        }

        int FromQuantifier(QuantifierToken quantifier)
        {
            if (quantifier.Max is null)
            {
                // one extra outgoing transition to handle "zero or more"
                return 1;
            }

            // quantifier has optional part
            return quantifier.Max > quantifier.Min ? 1 : 0;
        }
    }

    private RegexAutomatonState BuildMarkerAutomaton(
        MarkerToken marker,
        RegexAutomatonState startState,
        int endStateOutgoingTransitionsCount
    )
    {
        var endState = new RegexAutomatonState(NextStateId, endStateOutgoingTransitionsCount, 1);

        RegexAutomaton.AddTransition(
            new RegexAutomatonTransition(
                NextTransitionId,
                startState,
                endState,
                new MarkerPayload(marker.Marker)
            )
        );

        return endState;
    }

    private RegexAutomatonState BuildQuantifiableAutomaton(
        QuantifiableBranchItemToken branchItem,
        RegexAutomatonState startState,
        int endStateOutgoingTransitionsCount,
        HashSet<string> dependencies
    )
    {
        var min = branchItem.Quantifier.Min;
        var max = branchItem.Quantifier.Max;

        // this will help us predict best capacities for incoming and outgoing transitions collections
        var lastIndexInMandatoryChain = min - 1;
        var lastIndexInChain = max - 1;
        var lastMandatoryStateAdditionalOutgoingTransitionsCount = max > min ? 1 : 0;

        // here we store payloads which are created once, and used in many (quantified) states
        ITransitionPayload? singlePayload = null;
        (ITransitionPayload, ITransitionPayload)? pairedPayloads = null;

        var previousState = startState;

        for (var i = 0; i < min; i++)
        {
            var outgoingTransitionsCount = 1;

            if (i == lastIndexInChain)
            {
                outgoingTransitionsCount = endStateOutgoingTransitionsCount;
            }

            if (i == lastIndexInMandatoryChain)
            {
                outgoingTransitionsCount += lastMandatoryStateAdditionalOutgoingTransitionsCount;
            }

            previousState = BuildQuantifiable(previousState, outgoingTransitionsCount);
        }

        if (max is null)
        {
            var wrapperStartState = previousState;

            var quantifiableStartState = new RegexAutomatonState(
                NextStateId,
                GetOutgoingTransitionsCount(branchItem),
                2
            );

            var quantifiableEndState = BuildQuantifiable(quantifiableStartState, 2);

            previousState = BuildZeroOrMoreAutomaton(wrapperStartState, quantifiableStartState, quantifiableEndState, endStateOutgoingTransitionsCount);
        }
        else
        {
            var optionalChainLength = max.Value - min;
            if (optionalChainLength == 1)
            {
                var endState = BuildQuantifiable(
                    previousState,
                    endStateOutgoingTransitionsCount,
                    1
                );

                BuildOptionalAutomaton(previousState, endState);

                previousState = endState;
            }
            else if (optionalChainLength > 1)
            {
                var veryEndState = new RegexAutomatonState(
                    NextStateId,
                    0,
                    optionalChainLength
                );

                for (var i = min; i < max.Value; i++)
                {
                    var endState = BuildQuantifiable(
                        previousState,
                        i == lastIndexInChain ? endStateOutgoingTransitionsCount : 1,
                        i == lastIndexInChain ? optionalChainLength : 0
                    );

                    BuildOptionalAutomaton(previousState, veryEndState);

                    previousState = endState;
                }

                veryEndState.MoveIncomingTransitionsToOtherState(previousState);
            }
        }

        return previousState;

        RegexAutomatonState BuildQuantifiable(
            RegexAutomatonState quantifiableStartState,
            int quantifiableEndStateOutgoingTransitionsCount,
            int additionalQuantifiableEndStateIncomingTransitionsCount = 0
        )
        {
            var quantifiableToken = branchItem.Quantifiable;

            if (quantifiableToken is RegexGroupToken groupToken)
            {
                return BuildGroupAutomaton(
                    groupToken,
                    dependencies,
                    (
                        quantifiableStartState,
                        quantifiableEndStateOutgoingTransitionsCount,
                        additionalQuantifiableEndStateIncomingTransitionsCount
                    )
                ).EndState;
            }

            if (quantifiableToken is IRuleReferenceToken ruleReferenceToken)
            {
                var variableName = branchItem.VariableName;

                if (variableName is null)
                {
                    singlePayload ??= CreateRuleReferencePayload(ruleReferenceToken);
                }
                else
                {
                    pairedPayloads ??= CreateNerPayloads(variableName, ruleReferenceToken);
                }
                dependencies.Add(ruleReferenceToken.GetRuleSpaceKey());
            }
            else if (quantifiableToken is ITerminalToken terminalToken)
            {
                singlePayload ??= CreateTerminalPayload(terminalToken);
            }

            if (singlePayload is not null)
            {
                var quantifiableEndState = new RegexAutomatonState(
                    NextStateId,
                    quantifiableEndStateOutgoingTransitionsCount,
                    1 + additionalQuantifiableEndStateIncomingTransitionsCount
                );

                RegexAutomaton.AddTransition(
                    new RegexAutomatonTransition(
                        NextTransitionId,
                        quantifiableStartState,
                        quantifiableEndState,
                        singlePayload
                    )
                );

                return quantifiableEndState;
            }

            if (pairedPayloads is not null)
            {
                var middleState = new RegexAutomatonState(NextStateId, 1, 1);

                RegexAutomaton.AddTransition(
                    new RegexAutomatonTransition(
                        NextTransitionId,
                        quantifiableStartState,
                        middleState,
                        pairedPayloads.Value.Item1
                    )
                );

                var quantifiableEndState = new RegexAutomatonState(
                    NextStateId,
                    quantifiableEndStateOutgoingTransitionsCount,
                    1 + additionalQuantifiableEndStateIncomingTransitionsCount
                );

                RegexAutomaton.AddTransition(
                    new RegexAutomatonTransition(
                        NextTransitionId,
                        middleState,
                        quantifiableEndState,
                        pairedPayloads.Value.Item2
                    )
                );

                return quantifiableEndState;
            }

            throw new RegexProcessorBuildException(
                $"Unknown quantifiable token type '{quantifiableToken.GetType().FullName}'."
            );
        }
    }

    private RuleReferencePayload CreateRuleReferencePayload(IRuleReferenceToken ruleReference)
    {
        return new RuleReferencePayload(ruleReference, _ruleSpace);
    }

    private (NerPayload, VariableCapturePayload) CreateNerPayloads(
        string variableName,
        IRuleReferenceToken ruleReference
    )
    {
        return (new NerPayload(ruleReference, _ruleSpace), new VariableCapturePayload(variableName));
    }

    private TerminalPayload CreateTerminalPayload(ITerminalToken terminal)
    {
        return new TerminalPayload(
            terminal switch
            {
                AnyLiteralToken => AnyLiteralDetector.Instance,
                LiteralSetToken literalSet => new LiteralSetDetector(literalSet),
                LiteralToken literal => new LiteralDetector(literal),
                PrefixToken prefix => new PrefixDetector(prefix),
                InfixToken infix => new InfixDetector(infix),
                SuffixToken suffix => new SuffixDetector(suffix),
                _ => throw new RegexProcessorBuildException(
                    $"Unknown terminal token type '{terminal.GetType().FullName}'."
                ),
            }
        );
    }

    private void BuildOptionalAutomaton(RegexAutomatonState startState, RegexAutomatonState endState)
    {
        RegexAutomaton.AddTransition(
            new RegexAutomatonTransition(
                NextTransitionId,
                startState,
                endState,
                EpsilonPayload.Instance
            )
        );
    }

    private RegexAutomatonState BuildZeroOrMoreAutomaton(
        RegexAutomatonState wrapperStartState,
        RegexAutomatonState startState,
        RegexAutomatonState endState,
        int outgoingTransitionsCount
    )
    {
        var wrapperEndState = new RegexAutomatonState(NextStateId, outgoingTransitionsCount, 2);

        BuildOptionalAutomaton(wrapperStartState, wrapperEndState);

        RegexAutomaton.AddTransition(
            new RegexAutomatonTransition(
                NextTransitionId,
                wrapperStartState,
                startState,
                EpsilonPayload.Instance
            )
        );

        RegexAutomaton.AddTransition(
            new RegexAutomatonTransition(
                NextTransitionId,
                endState,
                wrapperEndState,
                EpsilonPayload.Instance
            )
        );

        RegexAutomaton.AddTransition(
            new RegexAutomatonTransition(
                NextTransitionId,
                endState,
                startState,
                EpsilonPayload.Instance
            )
        );

        return wrapperEndState;
    }
}