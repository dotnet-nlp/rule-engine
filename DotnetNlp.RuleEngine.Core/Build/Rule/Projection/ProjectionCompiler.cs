using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using DotnetNlp.RuleEngine.Core.Build.Rule.Projection.Models;
using DotnetNlp.RuleEngine.Core.Evaluation.Rule.Projection;
using DotnetNlp.RuleEngine.Core.Evaluation.Rule.Projection.Parameters;
using DotnetNlp.RuleEngine.Core.Exceptions;
using DotnetNlp.RuleEngine.Core.Lib.CodeAnalysis;
using DotnetNlp.RuleEngine.Core.Lib.CodeAnalysis.Assemblies;
using DotnetNlp.RuleEngine.Core.Lib.CodeAnalysis.Models;
using DotnetNlp.RuleEngine.Core.Lib.CodeAnalysis.Types.Formatting;
using DotnetNlp.RuleEngine.Core.Lib.Common.Helpers;

namespace DotnetNlp.RuleEngine.Core.Build.Rule.Projection;

internal sealed class ProjectionCompiler : IProjectionCompiler
{
    private readonly ITypeFormatter _typeFormatter;
    private readonly CodeEmitter _codeEmitter;

    public ProjectionCompiler(ITypeFormatter typeFormatter, CodeEmitter codeEmitter)
    {
        _typeFormatter = typeFormatter;
        _codeEmitter = codeEmitter;
    }

    public IRuleProjection CreateProjection(
        string ruleKey,
        IProjectionCompilationData data,
        IAssembliesProvider assembliesProvider
    )
    {
        return data switch
        {
            BodyBasedProjectionCompilationData bodyBased => CreateFunctionBasedProjection(
                bodyBased.ProjectionParameters,
                _codeEmitter.CreateFunction(GetFunctionCreationData(ruleKey, bodyBased), assembliesProvider)
            ),
            MatchedInputBasedProjectionCompilationData => MatchedInputBasedProjection.Instance,
            VoidProjectionCompilationData => VoidProjection.Instance,
            ConstantProjectionCompilationData constant => new ConstantProjection(constant.Constant),
            _ => throw new ArgumentOutOfRangeException(nameof(data)),
        };
    }
    public Dictionary<string, IRuleProjection> CreateProjections(
        IDictionary<string, IProjectionCompilationData> dataByRuleName,
        IAssembliesProvider assembliesProvider,
        int extraCapacity = 0
    )
    {
        var projections = new Dictionary<string, IRuleProjection>(dataByRuleName.Count + extraCapacity);
        var bodyBasedDataByRuleName = new Dictionary<string, BodyBasedProjectionCompilationData>(dataByRuleName.Count);

        foreach (var (ruleName, data) in dataByRuleName)
        {
            if (data is BodyBasedProjectionCompilationData bodyBasedData)
            {
                bodyBasedDataByRuleName.Add(ruleName, bodyBasedData);
            }
            else
            {
                projections.Add(
                    ruleName,
                    data switch
                    {
                        MatchedInputBasedProjectionCompilationData => MatchedInputBasedProjection.Instance,
                        VoidProjectionCompilationData => VoidProjection.Instance,
                        ConstantProjectionCompilationData constant => new ConstantProjection(constant.Constant),
                        _ => throw new ArgumentOutOfRangeException(nameof(data)),
                    }
                );
            }
        }

        var bodyBasedProjections = CreateBodyBasedProjections(bodyBasedDataByRuleName, assembliesProvider);

        foreach (var (ruleName, projection) in bodyBasedProjections)
        {
            projections.Add(ruleName, projection);
        }

        return projections;
    }

    private IEnumerable<KeyValuePair<string, IRuleProjection>> CreateBodyBasedProjections(
        IReadOnlyDictionary<string, BodyBasedProjectionCompilationData> dataByRuleName,
        IAssembliesProvider assembliesProvider
    )
    {
        if (dataByRuleName.Count == 0)
        {
            return ImmutableDictionary<string, IRuleProjection>.Empty;
        }

        try
        {
            IReadOnlyDictionary<string, FunctionCreationData> functionCreationDataByName = dataByRuleName
                .MapValue(GetFunctionCreationData)
                .ToDictionaryWithKnownCapacity(dataByRuleName.Count);

            return _codeEmitter
                .CreateFunctions(functionCreationDataByName, assembliesProvider)
                .MapValue(
                    (ruleKey, function) => CreateFunctionBasedProjection(
                        dataByRuleName[ruleKey].ProjectionParameters,
                        function
                    )
                );
        }
        catch (Exception exception)
        {
            throw new RuleBuildException("Projection compilation error", exception);
        }
    }

    private FunctionCreationData GetFunctionCreationData(string ruleKey, BodyBasedProjectionCompilationData data)
    {
        return new FunctionCreationData(
            data.Usings,
            _typeFormatter.GetStringRepresentation(data.ResultType),
            ruleKey.Replace('.', '_'),
            data
                .ProjectionParameters
                .Values
                .WhereValue(parameterType => parameterType != typeof(void))
                .Select(
                    parameterPair => new VariableCreationData(
                        parameterPair.Key,
                        _typeFormatter.GetStringRepresentation(parameterPair.Value)
                    )
                ),
            data.Body
        );
    }

    private static IRuleProjection CreateFunctionBasedProjection(
        ProjectionParameters projectionParameters,
        Func<object?[], object?> function
    )
    {
        return new FunctionBasedProjection(projectionParameters, function);
    }
}