using System;
using System.Collections.Generic;
using System.Linq;
using DotnetNlp.RuleEngine.Core.Build;
using DotnetNlp.RuleEngine.Core.Build.Composition;
using DotnetNlp.RuleEngine.Core.Build.InputProcessing;
using DotnetNlp.RuleEngine.Core.Build.InputProcessing.Models;
using DotnetNlp.RuleEngine.Core.Build.Tokenization.Tokens;
using DotnetNlp.RuleEngine.Core.Evaluation;
using DotnetNlp.RuleEngine.Core.Evaluation.InputProcessing;
using DotnetNlp.RuleEngine.Core.Evaluation.Rule.Result.SelectionStrategy;
using DotnetNlp.RuleEngine.Core.Exceptions;
using DotnetNlp.RuleEngine.Core.Lib.Common.Helpers;
using DotnetNlp.RuleEngine.Mechanics.Peg.Build.InputProcessing.Grammar;
using DotnetNlp.RuleEngine.Mechanics.Peg.Build.Tokenization.Tokens;
using DotnetNlp.RuleEngine.Mechanics.Peg.Evaluation.InputProcessing;
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
        IRuleSpaceDescription ruleSpaceDescription,
        Action<Action> subscribeOnRuleSpaceCreated
    )
    {
        var groupToken = (PegGroupToken) patternToken;

        try
        {
            var (root, dependencies, dependenciesOnRuleSpaceParameters) = new PegGrammarRuleBuilder(
                groupToken,
                ruleSpaceDescription,
                ruleSpace,
                _bestReferenceSelectionStrategy,
                subscribeOnRuleSpaceCreated
            ).Build();

            return new PegProcessor(new RuleDependenciesProvider(dependencies, dependenciesOnRuleSpaceParameters, ruleSpace.Id, ruleSpace.Name), root);
        }
        catch (RuleBuildException exception) when (exception is not PegProcessorBuildException)
        {
            throw;
        }
        catch (Exception exception) when (exception is not PegProcessorBuildException)
        {
            throw new PegProcessorBuildException($"Failed to create {nameof(PegProcessor)}.", exception);
        }
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
            if (type.IsValueType && !type.IsNullable())
            {
                return typeof(Nullable<>).MakeGenericType(type);
            }

            return type;
        }
    }
}