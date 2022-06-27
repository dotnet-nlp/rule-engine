using System;

namespace DotnetNlp.RuleEngine.Core.Build.Rule.Static.Attributes;

[AttributeUsage(AttributeTargets.Method)]
public sealed class StaticRuleAttribute: Attribute
{
    public string Name { get; }
    public string UsedWordsProviderMethodName { get; }

    public StaticRuleAttribute(string name, string usedWordsProviderMethodName)
    {
        Name = name;
        UsedWordsProviderMethodName = usedWordsProviderMethodName;
    }
}