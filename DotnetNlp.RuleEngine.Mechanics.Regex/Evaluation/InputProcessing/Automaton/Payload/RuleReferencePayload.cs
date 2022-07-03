using System.Collections.Generic;
using DotnetNlp.RuleEngine.Core.Build.Tokenization.Tokens;
using DotnetNlp.RuleEngine.Core.Build.Tokenization.Tokens.Arguments;
using DotnetNlp.RuleEngine.Core.Evaluation;
using DotnetNlp.RuleEngine.Core.Evaluation.ArgumentsBinding;
using DotnetNlp.RuleEngine.Core.Evaluation.Cache;
using DotnetNlp.RuleEngine.Core.Evaluation.Rule;
using DotnetNlp.RuleEngine.Core.Evaluation.Rule.Input;
using DotnetNlp.RuleEngine.Core.Lib.Common.Helpers;
using DotnetNlp.RuleEngine.Mechanics.Regex.Evaluation.InputProcessing.Automaton.Models;
using DotnetNlp.RuleEngine.Mechanics.Regex.Evaluation.InputProcessing.Automaton.Models.States;

namespace DotnetNlp.RuleEngine.Mechanics.Regex.Evaluation.InputProcessing.Automaton.Payload;

internal sealed class RuleReferencePayload : IRuleReferencePayload
{
    public bool IsTransient => false;

    public string RuleSpaceKey { get; }
    public readonly IRuleArgumentToken[] RuleArguments;
    private readonly IRuleSpace _ruleSpace;

    private IRuleMatcher? _matcher;
    private IRuleMatcher Matcher => _matcher ??= _ruleSpace[RuleSpaceKey];

    public RuleReferencePayload(IRuleReferenceToken ruleReference, IRuleSpace ruleSpace)
    {
        RuleSpaceKey = ruleReference.GetRuleSpaceKey();
        RuleArguments = ruleReference.Arguments;
        _ruleSpace = ruleSpace;
    }

    public void Consume(
        RuleInput input,
        RegexAutomatonState targetState,
        AutomatonProgress currentProgress,
        in Stack<AutomatonProgress> progresses,
        IRuleSpaceCache? cache = null
    )
    {
        var arguments = ArgumentsBinder.BindRuleArguments(
            Matcher.Parameters,
            input.RuleSpaceArguments,
            RuleArguments
        );

        var resultCollection = Matcher.Parameters.Values.Count == 0
            ? Matcher
                .Match(
                    input,
                    currentProgress.LastUsedSymbolIndex + 1,
                    arguments,
                    cache
                )
            : Matcher
                .MatchAndProject(
                    input,
                    currentProgress.LastUsedSymbolIndex + 1,
                    arguments,
                    cache
                );

        if (resultCollection.Count == 0)
        {
            return;
        }

        foreach (var result in resultCollection)
        {
            progresses.Push(
                currentProgress.Clone(
                    targetState,
                    replaceMarker: true,
                    marker: result.Marker,
                    lastUsedSymbolIndex: result.LastUsedSymbolIndex,
                    explicitlyMatchedSymbolsCount: currentProgress.ExplicitlyMatchedSymbolsCount + result.ExplicitlyMatchedSymbolsCount,
                    replaceCapturedVariables: true,
                    capturedVariables: currentProgress
                        .CapturedVariables
                        .MergeNullablesWithKnownCapacity(
                            result.CapturedVariables,
                            currentProgress.CapturedVariables?.Count + result.CapturedVariables?.Count ?? 0,
                            true
                        )
                )
            );
        }
    }

    public IEnumerable<string> GetUsedWords()
    {
        return Matcher.GetUsedWords();
    }
}