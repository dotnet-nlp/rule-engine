using System.Collections.Generic;
using System.Linq;

namespace DotnetNlp.RuleEngine.Core.Evaluation.Rule.Result.SelectionStrategy;

public sealed class CombinedStrategy : IResultSelectionStrategy
{
    private readonly IReadOnlyCollection<IResultSelectionStrategy> _strategies;

    public CombinedStrategy(IReadOnlyCollection<IResultSelectionStrategy> strategies)
    {
        _strategies = strategies;
    }

    public int Compare(RuleMatchResult a, RuleMatchResult b)
    {
        return _strategies
            .Select(strategy => strategy.Compare(a, b))
            .FirstOrDefault(result => result != 0);
    }
}