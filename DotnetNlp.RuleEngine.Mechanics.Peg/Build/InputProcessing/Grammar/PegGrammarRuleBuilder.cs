using System;
using System.Collections.Generic;
using System.Linq;
using DotnetNlp.RuleEngine.Core.Build;
using DotnetNlp.RuleEngine.Core.Build.Tokenization.Equality;
using DotnetNlp.RuleEngine.Core.Build.Tokenization.Tokens;
using DotnetNlp.RuleEngine.Core.Build.Tokenization.Tokens.Arguments;
using DotnetNlp.RuleEngine.Core.Evaluation;
using DotnetNlp.RuleEngine.Core.Evaluation.Rule.Result.SelectionStrategy;
using DotnetNlp.RuleEngine.Mechanics.Peg.Build.Tokenization.Tokens;
using DotnetNlp.RuleEngine.Mechanics.Peg.Evaluation.InputProcessing.Composers;
using DotnetNlp.RuleEngine.Mechanics.Peg.Evaluation.InputProcessing.Parsers;
using DotnetNlp.RuleEngine.Mechanics.Peg.Evaluation.InputProcessing.TerminalDetectors;
using DotnetNlp.RuleEngine.Mechanics.Peg.Exceptions;

namespace DotnetNlp.RuleEngine.Mechanics.Peg.Build.InputProcessing.Grammar;

internal sealed class PegGrammarRuleBuilder
{
    private readonly PegGroupToken _groupToken;
    private readonly IRuleSpaceDescription _ruleSpaceDescription;
    private readonly IRuleSpace _ruleSpace;
    private readonly IResultSelectionStrategy _bestReferenceSelectionStrategy;
    private readonly Action<Action> _subscribeOnRuleSpaceCreated;
    private readonly HashSet<string> _dependencies;
    private readonly HashSet<IChainedMemberAccessToken> _dependenciesOnRuleSpaceParameters;

    public PegGrammarRuleBuilder(
        PegGroupToken groupToken,
        IRuleSpaceDescription ruleSpaceDescription,
        IRuleSpace ruleSpace,
        IResultSelectionStrategy bestReferenceSelectionStrategy,
        Action<Action> subscribeOnRuleSpaceCreated
    )
    {
        _groupToken = groupToken;
        _ruleSpaceDescription = ruleSpaceDescription;
        _ruleSpace = ruleSpace;
        _bestReferenceSelectionStrategy = bestReferenceSelectionStrategy;
        _subscribeOnRuleSpaceCreated = subscribeOnRuleSpaceCreated;
        _dependencies = new HashSet<string>();
        _dependenciesOnRuleSpaceParameters = new HashSet<IChainedMemberAccessToken>(
            ChainedMemberAccessTokenEqualityComparer.Instance
        );
    }

    public (OrderedChoiceComposer Composer, IReadOnlySet<string> Dependencies, IReadOnlySet<IChainedMemberAccessToken> DependenciesOnRuleSpaceParameters) Build()
    {
        var composer = CreateGroupComposer(_groupToken, false);

        return (
            composer,
            _dependencies,
            _dependenciesOnRuleSpaceParameters
        );
    }

    private OrderedChoiceComposer CreateGroupComposer(PegGroupToken group, bool isPartOfNestedGroup)
    {
        return new OrderedChoiceComposer(
            group
                .Branches
                .Select(
                    branch => new SequenceComposer(
                        branch
                            .Items
                            .Select<BranchItemToken, IComposer>(
                                branchItem =>
                                {
                                    if (branchItem.VariableName is not null)
                                    {
                                        if (isPartOfNestedGroup)
                                        {
                                            throw new PegProcessorBuildException(
                                                $"Variable capturing is not allowed in nested groups " +
                                                $"(variable name '{branchItem.VariableName}')."
                                            );
                                        }

                                        if (branchItem.Quantifiable is PegGroupToken)
                                        {
                                            throw new PegProcessorBuildException(
                                                $"Group is not capturable " +
                                                $"(variable name '{branchItem.VariableName}')."
                                            );
                                        }

                                        if (branchItem.Lookahead is not null)
                                        {
                                            throw new PegProcessorBuildException(
                                                $"Capturing lookahead items is not allowed " +
                                                $"(variable name '{branchItem.VariableName}')."
                                            );
                                        }
                                    }

                                    if (branchItem.Quantifier.Min < 0)
                                    {
                                        throw new PegProcessorBuildException(
                                            $"Min value of quantifier must be greater or equal to zero, " +
                                            $"'{branchItem.Quantifier}' given."
                                        );
                                    }

                                    if (branchItem.Quantifier.Max < 1)
                                    {
                                        throw new PegProcessorBuildException(
                                            $"Max value of quantifier must be greater or equal to one, " +
                                            $"'{branchItem.Quantifier}' given."
                                        );
                                    }

                                    if (branchItem.Quantifier.Min > branchItem.Quantifier.Max)
                                    {
                                        throw new PegProcessorBuildException(
                                            $"Max value of quantifier must be greater or equal to min value, " +
                                            $"'{branchItem.Quantifier}' given."
                                        );
                                    }

                                    var quantifiedPieceComposer = new QuantifiedPieceComposer(
                                        branchItem.Quantifiable switch
                                        {
                                            ITerminalToken terminalToken => new TerminalParser(
                                                terminalToken switch
                                                {
                                                    AnyLiteralToken => AnyLiteralDetector.Instance,
                                                    LiteralSetToken literalSet => new LiteralSetDetector(literalSet),
                                                    LiteralToken literal => new LiteralDetector(literal),
                                                    PrefixToken prefix => new PrefixDetector(prefix),
                                                    InfixToken infix => new InfixDetector(infix),
                                                    SuffixToken suffix => new SuffixDetector(suffix),
                                                    _ => throw new PegProcessorBuildException(
                                                        $"Unknown terminal type {terminalToken.GetType().FullName}."
                                                    ),
                                                }
                                            ),
                                            RuleReferenceToken ruleReferenceToken => CreateRuleReferenceParser(
                                                ruleReferenceToken
                                            ),
                                            PegGroupToken groupToken => new GroupParser(
                                                CreateGroupComposer(groupToken, true)
                                            ),
                                            _ => throw new PegProcessorBuildException(
                                                $"Unknown quantifiable type " +
                                                $"{branchItem.Quantifiable.GetType().FullName}."
                                            ),
                                        },
                                        branchItem.Quantifier,
                                        branchItem.VariableName
                                    );

                                    if (branchItem.Lookahead is not null)
                                    {
                                        return new LookaheadComposer(branchItem.Lookahead, quantifiedPieceComposer);
                                    }

                                    return quantifiedPieceComposer;
                                }
                            )
                            .ToArray()
                    )
                )
                .ToArray()
        );
    }

    private RuleReferenceParser CreateRuleReferenceParser(IRuleReferenceToken ruleReferenceToken)
    {
        var ruleSpaceKey = ruleReferenceToken.GetRuleSpaceKey();

        _ruleSpaceDescription.ThrowIfNotExists(ruleSpaceKey);

        var parser = new RuleReferenceParser(
            _ruleSpaceDescription[ruleSpaceKey],
            ruleReferenceToken,
            _bestReferenceSelectionStrategy
        );

        _subscribeOnRuleSpaceCreated(() => parser.SetMatcher(_ruleSpace[ruleSpaceKey]));
        _dependencies.Add(ruleSpaceKey);
        _dependenciesOnRuleSpaceParameters.UnionWith(ruleReferenceToken.Arguments.OfType<IChainedMemberAccessToken>());

        return parser;
    }
}