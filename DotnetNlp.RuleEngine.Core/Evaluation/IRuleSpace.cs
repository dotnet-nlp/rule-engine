using System;
using System.Collections.Generic;
using DotnetNlp.RuleEngine.Core.Evaluation.Rule;

namespace DotnetNlp.RuleEngine.Core.Evaluation;

public interface IRuleSpace : IDictionary<string, IRuleMatcher>
{
    IReadOnlyDictionary<string, Type> RuleSpaceParameterTypesByName { get; }
    IReadOnlyDictionary<string, Type> RuleResultTypesByName { get; }
    IReadOnlyDictionary<string, IRuleMatcher> RuleMatchersByName { get; }
}