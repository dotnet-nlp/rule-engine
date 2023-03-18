using System;
using System.Collections.Generic;
using System.Collections.Immutable;

namespace DotnetNlp.RuleEngine.Core.Evaluation.Rule.Projection.Parameters;

public sealed class RuleSpaceParameters
{
    public static readonly RuleSpaceParameters Empty = new(ImmutableDictionary<string, Type>.Empty);

    public IReadOnlyDictionary<string, Type> Values { get; }

    public RuleSpaceParameters(IReadOnlyDictionary<string, Type> values)
    {
        Values = values;
    }
}