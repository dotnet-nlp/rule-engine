using System;
using System.Collections.Generic;
using DotnetNlp.RuleEngine.Core.Build.Tokenization.Tokens;
using DotnetNlp.RuleEngine.Core.Build.Tokenization.Tokens.Arguments;
using DotnetNlp.RuleEngine.Core.Evaluation;
using DotnetNlp.RuleEngine.Core.Evaluation.ArgumentsBinding;
using DotnetNlp.RuleEngine.Core.Evaluation.Cache;
using DotnetNlp.RuleEngine.Core.Evaluation.Rule;
using DotnetNlp.RuleEngine.Core.Evaluation.Rule.Projection.Arguments;
using DotnetNlp.RuleEngine.Core.Evaluation.Rule.Result.SelectionStrategy;

namespace DotnetNlp.RuleEngine.Mechanics.Peg.Evaluation.InputProcessing.Parsers;

internal sealed class RuleReferenceParser : IQuantifiableParser
{
    public Type ResultType => Matcher.ResultDescription.ResultType;

    public string RuleSpaceKey { get; }
    private readonly IRuleArgumentToken[] _ruleArguments;
    private readonly IResultSelectionStrategy _resultSelectionStrategy;
    private readonly IRuleSpace _ruleSpace;

    private IRuleMatcher? _matcher;
    private IRuleMatcher Matcher => _matcher ??= _ruleSpace[RuleSpaceKey];

    public RuleReferenceParser(
        IRuleReferenceToken ruleReferenceToken,
        IResultSelectionStrategy resultSelectionStrategy,
        IRuleSpace ruleSpace
    )
    {
        RuleSpaceKey = ruleReferenceToken.GetRuleSpaceKey();
        _ruleArguments = ruleReferenceToken.Arguments;
        _resultSelectionStrategy = resultSelectionStrategy;
        _ruleSpace = ruleSpace;
    }

    public bool TryParse(
        string[] sequence,
        ref int index,
        out int explicitlyMatchedSymbolsCount,
        out object? result,
        RuleSpaceArguments? ruleSpaceArguments = null,
        IRuleSpaceCache? cache = null
    )
    {
        var matchResult = Matcher
            .MatchAndProject(
                sequence,
                index,
                ruleSpaceArguments,
                ArgumentsBinder.BindRuleArguments(
                    Matcher.Parameters,
                    _ruleArguments,
                    ruleSpaceArguments
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