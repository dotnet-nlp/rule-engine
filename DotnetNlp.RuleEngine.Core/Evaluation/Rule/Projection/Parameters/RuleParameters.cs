using System;
using System.Collections.Generic;

namespace DotnetNlp.RuleEngine.Core.Evaluation.Rule.Projection.Parameters;

public sealed class RuleParameters
{
    public IReadOnlyDictionary<string, Type> Values { get; }

    public RuleParameters(IReadOnlyDictionary<string, Type> values)
    {
        Values = values;
    }
}