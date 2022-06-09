using RuleEngine.Core.Evaluation;
using RuleEngine.Core.Evaluation.Rule;

namespace RuleEngine.Core.Build.Rule.Source;

internal interface IRuleSource
{
    IRuleMatcher GetRuleMatcher(in IRuleSpace ruleSpace);
}