using System;
using System.Collections.Generic;
using RuleEngine.Core.Evaluation.Rule;

namespace RuleEngine.Core.Evaluation;

public interface IRuleSpace
{
    IReadOnlyDictionary<string, Type> RuleSpaceParameterTypesByName { get; }
    IReadOnlyDictionary<string, Type> RuleResultTypesByName { get; }
    IReadOnlyDictionary<string, IRuleMatcher> RuleMatchersByName { get; }
    IRuleMatcher this[string ruleName] { get; set; }
}