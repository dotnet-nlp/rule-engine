﻿using System.Collections.Generic;
using RuleEngine.Core.Build.Tokenization.Tokens;
using RuleEngine.Core.Build.Tokenization.Tokens.Arguments;
using RuleEngine.Core.Evaluation;
using RuleEngine.Core.Evaluation.ArgumentsBinding;
using RuleEngine.Core.Evaluation.Cache;
using RuleEngine.Core.Evaluation.Rule;
using RuleEngine.Core.Evaluation.Rule.Input;
using RuleEngine.Mechanics.Regex.Evaluation.InputProcessing.Automaton.Models;
using RuleEngine.Mechanics.Regex.Evaluation.InputProcessing.Automaton.Models.States;

namespace RuleEngine.Mechanics.Regex.Evaluation.InputProcessing.Automaton.Payload;

internal sealed class NerPayload : IRuleReferencePayload
{
    public bool IsTransient => false;
    public string RuleSpaceKey { get; }
    public readonly IRuleArgumentToken[] RuleArguments;
    private readonly IRuleSpace _ruleSpace;

    private IRuleMatcher? _matcher;
    private IRuleMatcher Matcher => _matcher ??= _ruleSpace[RuleSpaceKey];

    public NerPayload(IRuleReferenceToken ruleReference, IRuleSpace ruleSpace)
    {
        RuleSpaceKey = ruleReference.GetRuleSpaceKey();
        RuleArguments = ruleReference.Arguments;
        _ruleSpace = ruleSpace;
    }

    public void Consume(
        RuleInput input,
        RegexAutomatonState targetState,
        AutomatonProgress currentProgress,
        IRuleSpaceCache cache,
        in Stack<AutomatonProgress> progresses
    )
    {
        var resultCollection = Matcher
            .MatchAndProject(
                input,
                currentProgress.LastUsedSymbolIndex + 1,
                ArgumentsBinder.BindRuleArguments(
                    Matcher.Parameters,
                    input.RuleSpaceArguments,
                    RuleArguments
                ),
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
                    lastUsedSymbolIndex: result.LastUsedSymbolIndex,
                    explicitlyMatchedSymbolsCount: currentProgress.ExplicitlyMatchedSymbolsCount + result.ExplicitlyMatchedSymbolsCount,
                    replaceCapturedValueFactory: true,
                    capturedValueFactory: () => result.Result.Value
                )
            );
        }
    }

    public IEnumerable<string> GetUsedWords()
    {
        return Matcher.GetUsedWords();
    }
}