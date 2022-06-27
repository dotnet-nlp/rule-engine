using System;
using System.Collections.Generic;

namespace DotnetNlp.RuleEngine.Core.Evaluation.Rule.Projection.Parameters;

public sealed class RuleSpaceParameters
{
    public IReadOnlyDictionary<string, Type> Values { get; }

    public RuleSpaceParameters(IReadOnlyDictionary<string, Type> values)
    {
        Values = values;
    }
}