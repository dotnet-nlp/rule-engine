using System;
using System.Collections.Generic;
using RuleEngine.Core.Evaluation;

namespace RuleEngine.Core.Build;

public sealed class RuleSpaceBasedRuleSpaceDescription : IRuleSpaceDescription
{
    private readonly IRuleSpace _ruleSpace;

    public IReadOnlyDictionary<string, Type> ResultTypesByRuleName => _ruleSpace.RuleResultTypesByName;
    public Type this[string ruleKey] => _ruleSpace.RuleResultTypesByName[ruleKey];

    public RuleSpaceBasedRuleSpaceDescription(IRuleSpace ruleSpace)
    {
        _ruleSpace = ruleSpace;
    }
}