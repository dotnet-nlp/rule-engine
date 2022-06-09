namespace RuleEngine.Core.Evaluation.Cache;

public interface IManageableRuleSpaceCache : IRuleSpaceCache
{
    void Clear();
}