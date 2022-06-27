using System;

namespace DotnetNlp.RuleEngine.Core.Build.Rule.Static.Attributes;

[AttributeUsage(AttributeTargets.Class)]
public sealed class StaticRuleContainerAttribute: Attribute
{
    public string Namespace { get; }

    public StaticRuleContainerAttribute(string @namespace)
    {
        Namespace = @namespace;
    }
}