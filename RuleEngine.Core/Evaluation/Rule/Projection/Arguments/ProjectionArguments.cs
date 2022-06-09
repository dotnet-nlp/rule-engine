using System;
using System.Collections.Generic;
using System.Linq;
using RuleEngine.Core.Evaluation.Rule.Projection.Parameters;
using RuleEngine.Core.Lib.Common.Helpers;

namespace RuleEngine.Core.Evaluation.Rule.Projection.Arguments;

public sealed class ProjectionArguments
{
    public IReadOnlyCollection<string> Input { get; }
    private readonly CapturedVariablesArguments _capturedVariablesArguments;
    private readonly RuleArguments _ruleArguments;
    private readonly RuleSpaceArguments _ruleSpaceArguments;

    private IReadOnlyDictionary<string, object?>? _values;
    public IReadOnlyDictionary<string, object?> Values => _values ??= Array
        .Empty<KeyValuePair<string, object?>>()
        .Append(new KeyValuePair<string, object?>(ProjectionParameters.InputParameterName, Input))
        .Concat(_capturedVariablesArguments.Values)
        .Concat(_ruleArguments.Values)
        .Concat(_ruleSpaceArguments.Values)
        .ToDictionaryWithKnownCapacity(
            1 +
            _capturedVariablesArguments.Values.Count +
            _ruleArguments.Values.Count +
            _ruleSpaceArguments.Values.Count
        );

    public ProjectionArguments(
        IReadOnlyCollection<string> input,
        CapturedVariablesArguments capturedVariablesArguments,
        RuleArguments ruleArguments,
        RuleSpaceArguments ruleSpaceArguments
    )
    {
        Input = input;
        _capturedVariablesArguments = capturedVariablesArguments;
        _ruleArguments = ruleArguments;
        _ruleSpaceArguments = ruleSpaceArguments;
    }
}