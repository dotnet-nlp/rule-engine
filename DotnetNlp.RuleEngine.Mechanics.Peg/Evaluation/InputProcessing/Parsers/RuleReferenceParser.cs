using System;
using System.Collections.Generic;
using DotnetNlp.RuleEngine.Core.Build.Tokenization.Tokens;
using DotnetNlp.RuleEngine.Core.Build.Tokenization.Tokens.Arguments;
using DotnetNlp.RuleEngine.Core.Evaluation.Cache;
using DotnetNlp.RuleEngine.Core.Evaluation.Rule;
using DotnetNlp.RuleEngine.Core.Evaluation.Rule.Projection.Arguments;
using DotnetNlp.RuleEngine.Core.Evaluation.Rule.Result.SelectionStrategy;
using DotnetNlp.RuleEngine.Core.Lib.Common.Helpers;
using DotnetNlp.RuleEngine.Mechanics.Peg.Exceptions;

namespace DotnetNlp.RuleEngine.Mechanics.Peg.Evaluation.InputProcessing.Parsers;

internal sealed class RuleReferenceParser : IQuantifiableParser
{
    public Type ResultType { get; }
    private readonly IRuleArgumentToken[]? _ruleArguments;
    private readonly IResultSelectionStrategy _resultSelectionStrategy;

    private IRuleMatcher? _matcher;
    private IRuleMatcher Matcher => _matcher ?? throw new PegProcessorBuildException(
        $"{nameof(RuleReferenceParser)} is not initialized with {nameof(IRuleMatcher)}."
    );

    public RuleReferenceParser(
        Type resultType,
        IRuleReferenceToken ruleReferenceToken,
        IResultSelectionStrategy resultSelectionStrategy
    )
    {
        ResultType = resultType;
        _ruleArguments = ruleReferenceToken.Arguments.NullIfEmpty();
        _resultSelectionStrategy = resultSelectionStrategy;
    }

    public void SetMatcher(IRuleMatcher matcher)
    {
        _matcher = matcher;
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
                Matcher.Parameters.BindRuleArguments(_ruleArguments, ruleSpaceArguments),
                ruleSpaceArguments,
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
        yield break;
    }
}