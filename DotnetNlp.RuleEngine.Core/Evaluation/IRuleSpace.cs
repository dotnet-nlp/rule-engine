using System;
using System.Collections.Generic;
using DotnetNlp.RuleEngine.Core.Evaluation.Rule;
using DotnetNlp.RuleEngine.Core.Lib.Common.Helpers;

namespace DotnetNlp.RuleEngine.Core.Evaluation;

public interface IRuleSpace : IDictionary<string, IRuleMatcher>
{
    int Id { get; }
    string Name { get; }
    IReadOnlyDictionary<string, Type> RuleSpaceParameterTypesByName { get; }
    IReadOnlyDictionary<string, Type> RuleResultTypesByName { get; }
    IReadOnlySet<string> TransientRulesKeys { get; }
    IReadOnlySet<string> Prune(IReadOnlySet<string> rulesToPreserve);

    public IEnumerable<KeyValuePair<string, IRuleMatcher>> GetNonTransientRules()
    {
        return this.WhereKey(key => !TransientRulesKeys.Contains(key));
    }
}