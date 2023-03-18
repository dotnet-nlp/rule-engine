using System.Collections.Generic;
using System.Linq;
using DotnetNlp.RuleEngine.Core.Build.Tokenization.Tokens.Arguments;
using DotnetNlp.RuleEngine.Core.Evaluation.Cache;
using DotnetNlp.RuleEngine.Core.Evaluation.Rule.Projection.Arguments;
using DotnetNlp.RuleEngine.Core.Evaluation.Rule.Projection.Parameters;
using DotnetNlp.RuleEngine.Core.Evaluation.Rule.Result;
using DotnetNlp.RuleEngine.Core.Exceptions;
using DotnetNlp.RuleEngine.Core.Lib.Common.Helpers;

namespace DotnetNlp.RuleEngine.Core.Evaluation.Rule.Cache;

internal sealed class CachingRuleMatcher : IRuleMatcher
{
    private readonly int _id;
    private readonly IRuleMatcher _source;

    public RuleParameters Parameters => _source.Parameters;
    public RuleMatchResultDescription ResultDescription => _source.ResultDescription;

    public CachingRuleMatcher(int id, IRuleMatcher source)
    {
        _id = id;
        _source = source;
    }

    public IReadOnlySet<string> GetDependencies(IRuleSpace forRuleSpace)
    {
        return _source.GetDependencies(forRuleSpace);
    }

    public IReadOnlySet<IChainedMemberAccessToken>? GetDependenciesOnRuleSpaceParameters()
    {
        return _source.GetDependenciesOnRuleSpaceParameters();
    }

    public RuleMatchResultCollection Match(
        string[] sequence,
        int firstSymbolIndex = 0,
        RuleArguments? ruleArguments = null,
        RuleSpaceArguments? ruleSpaceArguments = null,
        IRuleSpaceCache? cache = null
    )
    {
        cache ??= new RuleSpaceCache();

        var dependenciesOnRuleSpaceParameters = _source.GetDependenciesOnRuleSpaceParameters();

        ThrowIfMissingArguments(dependenciesOnRuleSpaceParameters, ruleSpaceArguments);

        var dependenciesOnRuleSpaceArguments = ruleSpaceArguments?.GetValues(dependenciesOnRuleSpaceParameters);

        var matchResult = cache.GetResult(
            false,
            _id,
            sequence,
            firstSymbolIndex,
            ruleArguments?.Values,
            dependenciesOnRuleSpaceArguments
        );

        if (matchResult is not null)
        {
            return matchResult;
        }

        // todo [code quality] separate cache from IRuleMatcher
        matchResult = _source.Match(sequence, firstSymbolIndex, ruleArguments, ruleSpaceArguments, cache);

        cache.SetResult(
            false,
            _id,
            sequence,
            firstSymbolIndex,
            ruleArguments?.Values,
            dependenciesOnRuleSpaceArguments,
            matchResult
        );

        return matchResult;
    }

    public RuleMatchResultCollection MatchAndProject(
        string[] sequence,
        int firstSymbolIndex = 0,
        RuleArguments? ruleArguments = null,
        RuleSpaceArguments? ruleSpaceArguments = null,
        IRuleSpaceCache? cache = null
    )
    {
        cache ??= new RuleSpaceCache();

        var dependenciesOnRuleSpaceParameters = _source.GetDependenciesOnRuleSpaceParameters();

        ThrowIfMissingArguments(dependenciesOnRuleSpaceParameters, ruleSpaceArguments);

        var dependenciesOnRuleSpaceArguments = ruleSpaceArguments?.GetValues(dependenciesOnRuleSpaceParameters);

        var matchResult = cache.GetResult(
            true,
            _id,
            sequence,
            firstSymbolIndex,
            ruleArguments?.Values,
            dependenciesOnRuleSpaceArguments
        );

        if (matchResult is not null)
        {
            return matchResult;
        }

        // todo [realtime performance] we can try to get not projected result and project it
        // todo [code quality] separate cache from IRuleMatcher
        matchResult = _source.MatchAndProject(sequence, firstSymbolIndex, ruleArguments, ruleSpaceArguments, cache);

        cache.SetResult(
            false,
            _id,
            sequence,
            firstSymbolIndex,
            ruleArguments?.Values,
            dependenciesOnRuleSpaceArguments,
            matchResult
        );
        cache.SetResult(
            true,
            _id,
            sequence,
            firstSymbolIndex,
            ruleArguments?.Values,
            dependenciesOnRuleSpaceArguments,
            matchResult
        );

        return matchResult;
    }

    public IEnumerable<string> GetUsedWords()
    {
        return _source.GetUsedWords();
    }

    private void ThrowIfMissingArguments(
        IReadOnlySet<IChainedMemberAccessToken>? dependenciesOnRuleSpaceParameters,
        RuleSpaceArguments? ruleSpaceArguments
    )
    {
        if (dependenciesOnRuleSpaceParameters is not null && ruleSpaceArguments is null)
        {
            throw new RuleMatchException(
                $"Rule contains dependencies on missing rule space arguments: " +
                $"{dependenciesOnRuleSpaceParameters.Select(parameter => $"'{parameter}'").JoinToString(", ")}."
            );
        }
    }
}