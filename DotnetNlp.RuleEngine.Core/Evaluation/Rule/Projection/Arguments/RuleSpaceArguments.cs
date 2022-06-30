using System.Collections.Generic;
using System.Collections.Immutable;

namespace DotnetNlp.RuleEngine.Core.Evaluation.Rule.Projection.Arguments;

public sealed class RuleSpaceArguments
{
    public static readonly RuleSpaceArguments Empty = new(ImmutableDictionary<string, object?>.Empty);

    /// <remarks>
    /// Performance remarks: library performance depends on the way this field is declared.
    /// Please make sure you know what you are doing, when changing this field's declaration.
    /// </remarks>
    public readonly IReadOnlyDictionary<string, object?> Values;

    public RuleSpaceArguments(IReadOnlyDictionary<string, object?> values)
    {
        Values = values;
    }
}