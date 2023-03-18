﻿using System.Collections.Generic;
using DotnetNlp.RuleEngine.Core.Build.Tokenization.Tokens;
using DotnetNlp.RuleEngine.Core.Build.Tokenization.Tokens.Arguments;
using DotnetNlp.RuleEngine.Core.Evaluation.Cache;
using DotnetNlp.RuleEngine.Core.Evaluation.Rule;
using DotnetNlp.RuleEngine.Core.Evaluation.Rule.Projection.Arguments;
using DotnetNlp.RuleEngine.Core.Lib.Common.Helpers;
using DotnetNlp.RuleEngine.Mechanics.Regex.Evaluation.InputProcessing.Automaton.Models;
using DotnetNlp.RuleEngine.Mechanics.Regex.Evaluation.InputProcessing.Automaton.Models.States;
using DotnetNlp.RuleEngine.Mechanics.Regex.Exceptions;

namespace DotnetNlp.RuleEngine.Mechanics.Regex.Evaluation.InputProcessing.Automaton.Payload;

internal sealed class RuleReferencePayload : IRuleReferencePayload
{
    public bool IsTransient => false;

    public string RuleSpaceKey { get; }
    public readonly IRuleArgumentToken[]? RuleArguments;

    private IRuleMatcher? _matcher;
    private IRuleMatcher Matcher => _matcher ?? throw new RegexProcessorBuildException(
        $"{nameof(RuleReferencePayload)} is not initialized with {nameof(IRuleMatcher)}."
    );

    public RuleReferencePayload(IRuleReferenceToken ruleReference)
    {
        RuleSpaceKey = ruleReference.GetRuleSpaceKey();
        RuleArguments = ruleReference.Arguments.NullIfEmpty();
    }

    public void SetMatcher(IRuleMatcher matcher)
    {
        _matcher = matcher;
    }

    public void Consume(
        string[] sequence,
        RuleSpaceArguments? ruleSpaceArguments,
        RegexAutomatonState targetState,
        AutomatonProgress currentProgress,
        in Stack<AutomatonProgress> progresses,
        IRuleSpaceCache? cache = null
    )
    {
        var resultCollection = Matcher.Match(
            sequence,
            currentProgress.LastUsedSymbolIndex + 1,
            Matcher.Parameters.BindRuleArguments(RuleArguments, ruleSpaceArguments),
            ruleSpaceArguments,
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