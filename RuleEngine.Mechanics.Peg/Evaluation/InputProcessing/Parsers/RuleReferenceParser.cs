using System;
using System.Collections.Generic;
using System.Linq;
using RuleEngine.Core.Build.Tokenization.Tokens;
using RuleEngine.Core.Evaluation;
using RuleEngine.Core.Evaluation.ArgumentsBinding;
using RuleEngine.Core.Evaluation.Cache;
using RuleEngine.Core.Evaluation.Rule;
using RuleEngine.Core.Evaluation.Rule.Input;
using RuleEngine.Core.Evaluation.Rule.Result.SelectionStrategy;

namespace RuleEngine.Mechanics.Peg.Evaluation.InputProcessing.Parsers;

internal sealed class RuleReferenceParser : IQuantifiableParser
{
    public Type ResultType => _ruleSpace[RuleSpaceKey].ResultDescription.ResultType;

    private readonly IRuleReferenceToken _ruleReferenceToken;
    private readonly IResultSelectionStrategy _resultSelectionStrategy;
    private readonly IRuleSpace _ruleSpace;

    private IRuleMatcher? _matcher;
    private IRuleMatcher Matcher => _matcher ??= _ruleSpace[RuleSpaceKey];

    public string RuleSpaceKey => _ruleReferenceToken.GetRuleSpaceKey();

    public RuleReferenceParser(
        IRuleReferenceToken ruleReferenceToken,
        IResultSelectionStrategy resultSelectionStrategy,
        IRuleSpace ruleSpace
    )
    {
        _ruleReferenceToken = ruleReferenceToken;
        _resultSelectionStrategy = resultSelectionStrategy;
        _ruleSpace = ruleSpace;
    }

    public bool TryParse(
        RuleInput input,
        IRuleSpaceCache cache,
        ref int index,
        out int explicitlyMatchedSymbolsCount,
        out object? result
    )
    {
        var matchResult = Matcher
            .MatchAndProject(
                input,
                index,
                ArgumentsBinder.BindRuleArguments(
                    Matcher.Parameters,
                    input.RuleSpaceArguments,
                    _ruleReferenceToken.Arguments.ToArray()
                ),
                cache
            )
            .Best(_resultSelectionStrategy);

        if (matchResult is null)
        {
            explicitlyMatchedSymbolsCount = 0;
            result = null;

            return false;
        }

        index = matchResult.LastUsedSymbolIndex + 1;

        explicitlyMatchedSymbolsCount = matchResult.ExplicitlyMatchedSymbolsCount;
        result = matchResult.Result.Value;

        return true;
    }

    public IEnumerable<string> GetUsedWords()
    {
        // rule reference itself doesn't use any words
        yield break;
    }
}