using System;
using System.Collections.Generic;
using System.Linq;
using DotnetNlp.RuleEngine.Core.Build;
using DotnetNlp.RuleEngine.Core.Build.InputProcessing;
using DotnetNlp.RuleEngine.Core.Build.InputProcessing.Models;
using DotnetNlp.RuleEngine.Core.Build.Tokenization.Tokens;
using DotnetNlp.RuleEngine.Core.Evaluation;
using DotnetNlp.RuleEngine.Core.Evaluation.InputProcessing;
using DotnetNlp.RuleEngine.Core.Evaluation.Rule.Result.SelectionStrategy;
using DotnetNlp.RuleEngine.Mechanics.Peg.Build.Tokenization.Tokens;
using DotnetNlp.RuleEngine.Mechanics.Peg.Evaluation.InputProcessing;
using DotnetNlp.RuleEngine.Mechanics.Peg.Evaluation.InputProcessing.Composers;
using DotnetNlp.RuleEngine.Mechanics.Peg.Evaluation.InputProcessing.Parsers;
using DotnetNlp.RuleEngine.Mechanics.Peg.Evaluation.InputProcessing.TerminalDetectors;
using DotnetNlp.RuleEngine.Mechanics.Peg.Exceptions;

namespace DotnetNlp.RuleEngine.Mechanics.Peg.Build.InputProcessing;

public sealed class PegProcessorFactory : IInputProcessorFactory
{
    private readonly IResultSelectionStrategy _bestReferenceSelectionStrategy;

    public PegProcessorFactory(IResultSelectionStrategy bestReferenceSelectionStrategy)
    {
        _bestReferenceSelectionStrategy = bestReferenceSelectionStrategy;
    }

    public IInputProcessor Create(
        IPatternToken patternToken,
        IRuleSpace ruleSpace,
        IRuleSpaceDescription ruleSpaceDescription
    )
    {
        var pegGroupToken = (PegGroupToken) patternToken;

        return new PegProcessor(CreateGroupComposer(pegGroupToken, ruleSpace, false, ruleSpaceDescription));
    }

    public RuleCapturedVariables ExtractOwnCapturedVariables(
        IPatternToken patternToken,
        IRuleSpaceDescription ruleSpaceDescription
    )
    {
        var group = (PegGroupToken) patternToken;

        var variableTypeByVariableName = new SortedDictionary<string, Type>();

        var hasManyBranches = group.Branches.Length > 1;

        foreach (var branch in group.Branches)
        {
            foreach (var branchItem in branch.Items.Where(item => item.VariableName is not null))
            {
                var originalVariableType = GetVariableType(branchItem.Quantifiable);

                var actualVariableType = GetBranchesCountModifiedType(
                    GetQuantifiedType(originalVariableType, branchItem.Quantifier)
                );

                variableTypeByVariableName.Add(branchItem.VariableName!, actualVariableType);
            }
        }

        return new RuleCapturedVariables(variableTypeByVariableName, Array.Empty<string>());

        Type GetVariableType(IQuantifiableToken quantifiable)
        {
            return quantifiable switch
            {
                AnyLiteralToken => typeof(string),
                LiteralSetToken => typeof(string),
                LiteralToken => typeof(string),
                PrefixToken => typeof(string),
                InfixToken => typeof(string),
                SuffixToken => typeof(string),
                IRuleReferenceToken ruleReference => ruleSpaceDescription[ruleReference.GetRuleSpaceKey()],
                _ => throw new PegProcessorBuildException($"Unknown quantifiable type {quantifiable.GetType().FullName}."),
            };
        }

        Type GetQuantifiedType(Type type, QuantifierToken quantifier)
        {
            if (quantifier.Max is null or > 1)
            {
                return typeof(IReadOnlyCollection<>).MakeGenericType(type);
            }

            if (quantifier.Min == 0 && quantifier.Max == 1)
            {
                return MakeNullableIfPossible(type);
            }

            return type;
        }

        Type GetBranchesCountModifiedType(Type type)
        {
            return hasManyBranches ? MakeNullableIfPossible(type) : type;
        }

        Type MakeNullableIfPossible(Type type)
        {
            if (type.IsValueType && !IsNullable())
            {
                return typeof(Nullable<>).MakeGenericType(type);
            }

            return type;

            bool IsNullable()
            {
                return type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Nullable<>);
            }
        }
    }

    private OrderedChoiceComposer CreateGroupComposer(
        PegGroupToken group,
        IRuleSpace ruleSpace,
        bool isPartOfNestedGroup,
        IRuleSpaceDescription ruleSpaceDescription
    )
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
                                                ruleReferenceToken,
                                                ruleSpace,
                                                ruleSpaceDescription
                                            ),
                                            PegGroupToken groupToken => new GroupParser(
                                                CreateGroupComposer(groupToken, ruleSpace, true, ruleSpaceDescription)
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

    private RuleReferenceParser CreateRuleReferenceParser(
        IRuleReferenceToken ruleReferenceToken,
        IRuleSpace ruleSpace,
        IRuleSpaceDescription ruleSpaceDescription
    )
    {
        var parser = new RuleReferenceParser(
            ruleReferenceToken,
            _bestReferenceSelectionStrategy,
            ruleSpace
        );

        ruleSpaceDescription.ThrowIfNotExists(parser.RuleSpaceKey);

        return parser;
    }
}