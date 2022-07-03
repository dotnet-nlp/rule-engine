using System.Collections.Generic;
using DotnetNlp.RuleEngine.Core.Evaluation.Cache;
using DotnetNlp.RuleEngine.Core.Evaluation.Rule.Projection.Arguments;
using DotnetNlp.RuleEngine.Core.Evaluation.Rule.Projection.Parameters;
using DotnetNlp.RuleEngine.Core.Evaluation.Rule.Result;

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

    public RuleMatchResultCollection Match(
        string[] sequence,
        int firstSymbolIndex = 0,
        RuleSpaceArguments? ruleSpaceArguments = null,
        RuleArguments? ruleArguments = null,
        IRuleSpaceCache? cache = null
    )
    {
        cache ??= new RuleSpaceCache();

        var matchResult = cache.GetResult(_id, sequence, firstSymbolIndex, ruleArguments?.Values);

        if (matchResult is not null)
        {
            return matchResult;
        }

        // todo [code quality] separate cache from IRuleMatcher
        matchResult = _source.Match(sequence, firstSymbolIndex, ruleSpaceArguments, ruleArguments, cache);

        cache.SetResult(_id, sequence, firstSymbolIndex, null, matchResult);

        return matchResult;
    }

    public RuleMatchResultCollection MatchAndProject(
        string[] sequence,
        int firstSymbolIndex = 0,
        RuleSpaceArguments? ruleSpaceArguments = null,
        RuleArguments? ruleArguments = null,
        IRuleSpaceCache? cache = null
    )
    {
        cache ??= new RuleSpaceCache();

        var matchResult = cache.GetResult(_id, sequence, firstSymbolIndex, ruleArguments?.Values);

        if (matchResult is not null)
        {
            return matchResult;
        }

        // todo [code quality] separate cache from IRuleMatcher
        matchResult = _source.MatchAndProject(sequence, firstSymbolIndex, ruleSpaceArguments, ruleArguments, cache);

        cache.SetResult(_id, sequence, firstSymbolIndex, ruleArguments?.Values, matchResult);

        return matchResult;
    }

    public IEnumerable<string> GetUsedWords()
    {
        return _source.GetUsedWords();
    }
}