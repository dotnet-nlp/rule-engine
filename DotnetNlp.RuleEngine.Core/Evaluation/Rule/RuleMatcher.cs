using System;
using System.Collections.Generic;
using System.Linq;
using DotnetNlp.RuleEngine.Core.Evaluation.Cache;
using DotnetNlp.RuleEngine.Core.Evaluation.InputProcessing;
using DotnetNlp.RuleEngine.Core.Evaluation.Rule.Projection;
using DotnetNlp.RuleEngine.Core.Evaluation.Rule.Projection.Arguments;
using DotnetNlp.RuleEngine.Core.Evaluation.Rule.Projection.Parameters;
using DotnetNlp.RuleEngine.Core.Evaluation.Rule.Result;

namespace DotnetNlp.RuleEngine.Core.Evaluation.Rule;

internal sealed class RuleMatcher : IRuleMatcher
{
    private readonly IInputProcessor _inputProcessor;
    private readonly IRuleProjection _projection;
    private readonly CapturedVariablesParameters _capturedVariablesParameters;

    public IReadOnlySet<string> Dependencies => _inputProcessor.GetDependencies();
    public RuleParameters Parameters { get; }
    public RuleMatchResultDescription ResultDescription { get; }

    public RuleMatcher(
        IInputProcessor inputProcessor,
        RuleParameters ruleParameters,
        CapturedVariablesParameters capturedVariablesParameters,
        RuleMatchResultDescription resultDescription,
        IRuleProjection projection
    )
    {
        _inputProcessor = inputProcessor;
        Parameters = ruleParameters;
        _capturedVariablesParameters = capturedVariablesParameters;
        ResultDescription = resultDescription;
        _projection = projection;
    }

    public RuleMatchResultCollection Match(
        string[] sequence,
        int firstSymbolIndex = 0,
        RuleSpaceArguments? ruleSpaceArguments = null,
        RuleArguments? ruleArguments = null,
        IRuleSpaceCache? cache = null
    )
    {
        cache ??= new RuleSpaceCache();

        return _inputProcessor.Match(sequence, firstSymbolIndex, ruleSpaceArguments, cache);
    }

    public RuleMatchResultCollection MatchAndProject(
        string[] sequence,
        int firstSymbolIndex = 0,
        RuleSpaceArguments? ruleSpaceArguments = null,
        RuleArguments? ruleArguments = null,
        IRuleSpaceCache? cache = null
    )
    {
        var inputProcessorResult = Match(sequence, firstSymbolIndex, ruleSpaceArguments, ruleArguments, cache);

        return new RuleMatchResultCollection(
            inputProcessorResult
                .Select(
                    result => new RuleMatchResult(
                        result.Source,
                        result.FirstUsedSymbolIndex,
                        result.LastUsedSymbolIndex,
                        result.CapturedVariables,
                        result.ExplicitlyMatchedSymbolsCount,
                        result.Marker,
                        new Lazy<object?>(
                            () => ProjectionFactory.GetProjectionResult(
                                result,
                                _capturedVariablesParameters,
                                sequence,
                                _projection,
                                firstSymbolIndex,
                                ruleSpaceArguments,
                                ruleArguments
                            )
                        )
                    )
                ),
            inputProcessorResult.Count
        );
    }

    public IEnumerable<string> GetUsedWords()
    {
        return _inputProcessor.GetUsedWords();
    }
}