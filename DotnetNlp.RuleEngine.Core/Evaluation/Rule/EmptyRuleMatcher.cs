using System.Collections.Generic;
using DotnetNlp.RuleEngine.Core.Evaluation.Cache;
using DotnetNlp.RuleEngine.Core.Evaluation.Rule.Projection.Arguments;
using DotnetNlp.RuleEngine.Core.Evaluation.Rule.Projection.Parameters;
using DotnetNlp.RuleEngine.Core.Evaluation.Rule.Result;

namespace DotnetNlp.RuleEngine.Core.Evaluation.Rule;

internal sealed class EmptyRuleMatcher : IRuleMatcher
{
    public RuleParameters Parameters { get; }
    public RuleMatchResultDescription ResultDescription { get; }

    public EmptyRuleMatcher(RuleParameters parameters, RuleMatchResultDescription resultDescription)
    {
        Parameters = parameters;
        ResultDescription = resultDescription;
    }

    public RuleMatchResultCollection Match(
        string[] sequence,
        int firstSymbolIndex = 0,
        RuleSpaceArguments? ruleSpaceArguments = null,
        RuleArguments? ruleArguments = null,
        IRuleSpaceCache? cache = null
    )
    {
        return new RuleMatchResultCollection(0);
    }

    public RuleMatchResultCollection MatchAndProject(
        string[] sequence,
        int firstSymbolIndex = 0,
        RuleSpaceArguments? ruleSpaceArguments = null,
        RuleArguments? ruleArguments = null,
        IRuleSpaceCache? cache = null
    )
    {
        return Match(sequence, firstSymbolIndex, ruleSpaceArguments, ruleArguments, cache);
    }

    public IEnumerable<string> GetUsedWords()
    {
        yield break;
    }
}