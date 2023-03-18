using DotnetNlp.RuleEngine.Core.Evaluation.Rule.Result.SelectionStrategy;

namespace DotnetNlp.RuleEngine.Bundle;

public static class Strategy
{
    public static IResultSelectionStrategy Default = new CombinedStrategy(
        new IResultSelectionStrategy[]
        {
            new MaxExplicitSymbolsStrategy(),
            new MaxProgressStrategy(),
        }
    );
}