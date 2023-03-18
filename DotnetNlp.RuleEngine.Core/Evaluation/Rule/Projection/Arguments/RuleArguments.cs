using System;
using System.Collections.Generic;

namespace DotnetNlp.RuleEngine.Core.Evaluation.Rule.Projection.Arguments;

public sealed class RuleArguments
{
    public static readonly RuleArguments Empty = new(Array.Empty<KeyValuePair<string, object?>>());

    /// <remarks>
    /// Performance remarks: library performance depends on the way this field is declared.
    /// Please make sure you know what you are doing, when changing this field's declaration.
    /// </remarks>
    public readonly KeyValuePair<string, object?>[] Values;

    public RuleArguments(KeyValuePair<string, object?>[] values)
    {
        Values = values;
    }
}