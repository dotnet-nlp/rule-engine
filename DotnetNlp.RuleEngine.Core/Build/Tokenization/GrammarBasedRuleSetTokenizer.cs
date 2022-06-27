using System;
using System.Collections.Generic;
using System.Linq;
using DotnetNlp.RuleEngine.Core.Build.Tokenization.Grammar;
using DotnetNlp.RuleEngine.Core.Build.Tokenization.Tokens;
using DotnetNlp.RuleEngine.Core.Exceptions;
using DotnetNlp.RuleEngine.Core.Lib.CodeAnalysis.Tokenization.Grammar;
using DotnetNlp.RuleEngine.Core.Lib.CodeAnalysis.Tokenization.Tokens;
using IronMeta.Matcher;

namespace DotnetNlp.RuleEngine.Core.Build.Tokenization;

internal sealed class GrammarBasedRuleSetTokenizer : IRuleSetTokenizer
{
    private readonly IReadOnlyDictionary<string, IPatternTokenizer> _patternTokenizers;

    public GrammarBasedRuleSetTokenizer(IReadOnlyDictionary<string, IPatternTokenizer> patternTokenizers)
    {
        _patternTokenizers = patternTokenizers;
    }

    public RuleSetToken Tokenize(string ruleSet, string? @namespace, bool caseSensitive)
    {
        var matcher = new RuleSetSyntaxMatcher(
            @namespace,
            _patternTokenizers,
            new CSharpSyntaxMatcher(),
            caseSensitive
        );

        MatchResult<char, IToken> result;

        try
        {
            result = matcher.GetMatch(ruleSet.ToList(), matcher.RuleSet);
        }
        catch (RuleEngineTokenizationException)
        {
            throw;
        }
        catch (Exception exception)
        {
            throw new RuleEngineTokenizationException("Failed to parse rule set", exception, ruleSet);
        }

        if (!result.Success)
        {
            throw new RuleEngineTokenizationException("Failed to parse rule set", ruleSet);
        }

        if (result.Result is null)
        {
            throw new RuleEngineTokenizationException("Root token is null", ruleSet);
        }

        if (result.Result is not RuleSetToken ruleSetToken)
        {
            throw new RuleEngineTokenizationException($"Root token is of unexpected type '{result.Result.GetType().Name}'", ruleSet);
        }

        return ruleSetToken;
    }
}