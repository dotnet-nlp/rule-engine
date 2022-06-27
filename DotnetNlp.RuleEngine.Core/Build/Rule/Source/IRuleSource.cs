using DotnetNlp.RuleEngine.Core.Evaluation;
using DotnetNlp.RuleEngine.Core.Evaluation.Rule;

namespace DotnetNlp.RuleEngine.Core.Build.Rule.Source;

internal interface IRuleSource
{
    IRuleMatcher GetRuleMatcher(in IRuleSpace ruleSpace);
}