using System;
using System.Collections.Generic;
using System.Linq;
using DotnetNlp.RuleEngine.Core.Lib.Common.Helpers;

namespace DotnetNlp.RuleEngine.Core.Evaluation.Rule.Projection.Parameters;

internal sealed class ProjectionParameters
{
    public const string InputParameterName = "input";

    private readonly Type _inputParameter;
    private readonly CapturedVariablesParameters _capturedVariablesParameters;
    private readonly RuleParameters _ruleParameters;
    private readonly RuleSpaceParameters _ruleSpaceParameters;

    private IReadOnlyDictionary<string, Type>? _values;
    public IReadOnlyDictionary<string, Type> Values => _values ??= Array
        .Empty<KeyValuePair<string, Type>>()
        .Append(new KeyValuePair<string, Type>(InputParameterName, _inputParameter))
        .Concat(_capturedVariablesParameters.Values)
        .Concat(_ruleParameters.Values)
        .Concat(_ruleSpaceParameters.Values)
        .ToDictionaryWithKnownCapacity(
            1 +
            _capturedVariablesParameters.Values.Count +
            _ruleParameters.Values.Length +
            _ruleSpaceParameters.Values.Count
        );

    public ProjectionParameters(
        Type inputParameter,
        CapturedVariablesParameters capturedVariablesParameters,
        RuleParameters ruleParameters,
        RuleSpaceParameters ruleSpaceParameters
    )
    {
        _inputParameter = inputParameter;
        _capturedVariablesParameters = capturedVariablesParameters;
        _ruleParameters = ruleParameters;
        _ruleSpaceParameters = ruleSpaceParameters;
    }
}