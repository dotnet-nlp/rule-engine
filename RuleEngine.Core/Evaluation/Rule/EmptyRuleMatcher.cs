using System.Collections.Generic;
using RuleEngine.Core.Evaluation.Cache;
using RuleEngine.Core.Evaluation.Rule.Input;
using RuleEngine.Core.Evaluation.Rule.Projection.Arguments;
using RuleEngine.Core.Evaluation.Rule.Projection.Parameters;
using RuleEngine.Core.Evaluation.Rule.Result;

namespace RuleEngine.Core.Evaluation.Rule;

internal sealed class EmptyRuleMatcher : IRuleMatcher
{
    public RuleParameters Parameters { get; }
    public RuleMatchResultDescription ResultDescription { get; }

    public EmptyRuleMatcher(RuleParameters parameters, RuleMatchResultDescription resultDescription)
    {
        Parameters = parameters;
        ResultDescription = resultDescription;
    }

    public RuleMatchResultCollection Match(RuleInput input, int firstSymbolIndex, IRuleSpaceCache cache)
    {
        return new RuleMatchResultCollection(0);
    }

    public RuleMatchResultCollection MatchAndProject(
        RuleInput input,
        int firstSymbolIndex,
        RuleArguments ruleArguments,
        IRuleSpaceCache cache
    )
    {
        return Match(input, firstSymbolIndex, cache);
    }

    public IEnumerable<string> GetUsedWords()
    {
        yield break;
    }
}