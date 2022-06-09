namespace RuleEngine.Core.Evaluation.Rule.Result.SelectionStrategy;

public interface IResultSelectionStrategy
{
    int Compare(RuleMatchResult x, RuleMatchResult y);
}