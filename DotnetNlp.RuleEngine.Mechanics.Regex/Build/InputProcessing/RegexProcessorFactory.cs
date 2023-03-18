using System;
using System.Collections.Generic;
using System.Linq;
using DotnetNlp.RuleEngine.Core.Build;
using DotnetNlp.RuleEngine.Core.Build.InputProcessing;
using DotnetNlp.RuleEngine.Core.Build.InputProcessing.Models;
using DotnetNlp.RuleEngine.Core.Build.Tokenization.Tokens;
using DotnetNlp.RuleEngine.Core.Evaluation;
using DotnetNlp.RuleEngine.Core.Evaluation.InputProcessing;
using DotnetNlp.RuleEngine.Core.Exceptions;
using DotnetNlp.RuleEngine.Mechanics.Regex.Build.InputProcessing.Automaton;
using DotnetNlp.RuleEngine.Mechanics.Regex.Build.InputProcessing.Automaton.Optimization;
using DotnetNlp.RuleEngine.Mechanics.Regex.Build.Tokenization.Tokens;
using DotnetNlp.RuleEngine.Mechanics.Regex.Evaluation.InputProcessing;
using DotnetNlp.RuleEngine.Mechanics.Regex.Evaluation.InputProcessing.Automaton.Walker;
using DotnetNlp.RuleEngine.Mechanics.Regex.Exceptions;

namespace DotnetNlp.RuleEngine.Mechanics.Regex.Build.InputProcessing;

public sealed class RegexProcessorFactory : IInputProcessorFactory
{
    private readonly OptimizationLevel _optimizationLevel;

    public RegexProcessorFactory(OptimizationLevel optimizationLevel)
    {
        _optimizationLevel = optimizationLevel;
    }

    public IInputProcessor Create(
        IPatternToken patternToken,
        IRuleSpace ruleSpace,
        IRuleSpaceDescription ruleSpaceDescription
    )
    {
        var group = (RegexGroupToken) patternToken;

        try
        {
            var (automaton, dependencies) = new RegexAutomatonBuilder(group, ruleSpace).Build();

            RegexAutomatonPostprocessor.Instance.ValidateAndOptimize(
                automaton,
                _optimizationLevel,
                ruleSpaceDescription
            );

            return new RegexProcessor(automaton, RegexAutomatonWalker.Instance, dependencies);
        }
        catch (RuleBuildException exception) when (exception is not RegexProcessorBuildException)
        {
            throw;
        }
        catch (Exception exception) when (exception is not RegexProcessorBuildException)
        {
            throw new RegexProcessorBuildException($"Failed to create {nameof(RegexProcessor)}.", exception);
        }
    }

    public RuleCapturedVariables ExtractOwnCapturedVariables(
        IPatternToken patternToken,
        IRuleSpaceDescription ruleSpaceDescription
    )
    {
        var group = (RegexGroupToken) patternToken;

        var variables = new Dictionary<string, Type>();
        var references = new List<string>();

        RecursivelySearchForVariables(group, ruleSpaceDescription, variables, references);

        return new RuleCapturedVariables(variables, references);
    }

    private static void RecursivelySearchForVariables(
        RegexGroupToken group,
        IRuleSpaceDescription ruleSpaceDescription,
        in Dictionary<string, Type> collectedVariables,
        in List<string> collectedReferences
    )
    {
        var suitableBranchItems = group
            .Branches
            .SelectMany(branch => branch.Items)
            .OfType<QuantifiableBranchItemToken>();

        foreach (var branchItem in suitableBranchItems)
        {
            if (branchItem.VariableName is not null && branchItem.Quantifiable is RegexGroupToken)
            {
                throw new RuleBuildException("Group capturing is not allowed.");
            }

            if (branchItem.Quantifiable is RegexGroupToken regexGroupToken)
            {
                RecursivelySearchForVariables(
                    regexGroupToken,
                    ruleSpaceDescription,
                    collectedVariables,
                    collectedReferences
                );
            }
            else if (branchItem.VariableName is null && branchItem.Quantifiable is IRuleReferenceToken ruleReference)
            {
                collectedReferences.Add(ruleReference.GetRuleSpaceKey());
            }
            else if (branchItem.VariableName is not null)
            {
                var type = GetVariableType(branchItem.Quantifiable);

                if (collectedVariables.TryGetValue(branchItem.VariableName, out var existing))
                {
                    if (existing != type)
                    {
                        throw new RegexProcessorBuildException(
                            $"Variable '{branchItem.VariableName}' is declared with different types in different branches: " +
                            $"'{existing.FullName}' and '{type.FullName}'."
                        );
                    }
                }
                else
                {
                    collectedVariables.Add(branchItem.VariableName, type);
                }
            }
        }

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
                RuleReferenceToken ruleReference => GetRuleResultTypeDescription(ruleReference),
                _ => throw new RegexProcessorBuildException($"Unknown quantifiable type {quantifiable.GetType().FullName}."),
            };

            Type GetRuleResultTypeDescription(IRuleReferenceToken ruleReference)
            {
                var ruleKey = ruleReference.GetRuleSpaceKey();

                return ruleSpaceDescription.GetOrThrow(ruleKey);
            }
        }
    }
}