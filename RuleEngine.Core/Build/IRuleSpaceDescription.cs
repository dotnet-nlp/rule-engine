using System;
using System.Collections.Generic;
using RuleEngine.Core.Exceptions;

namespace RuleEngine.Core.Build;

public interface IRuleSpaceDescription
{
    IReadOnlyDictionary<string, Type> ResultTypesByRuleName { get; }

    Type this[string ruleKey] { get; }

    public Type GetOrThrow(string ruleKey)
    {
        ThrowIfNotExists(ruleKey);

        return ResultTypesByRuleName[ruleKey];
    }

    public void ThrowIfNotExists(string ruleKey)
    {
        if (!ResultTypesByRuleName.ContainsKey(ruleKey))
        {
            throw new RuleBuildException($"Rule with key '{ruleKey}' does not exist.");
        }
    }
}