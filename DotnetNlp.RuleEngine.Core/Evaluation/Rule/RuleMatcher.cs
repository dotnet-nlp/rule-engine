using System;
using System.Collections.Generic;
using System.Linq;
using DotnetNlp.RuleEngine.Core.Evaluation.Cache;
using DotnetNlp.RuleEngine.Core.Evaluation.InputProcessing;
using DotnetNlp.RuleEngine.Core.Evaluation.Rule.Input;
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
        RuleInput input,
        int firstSymbolIndex,
        RuleArguments ruleArguments,
        IRuleSpaceCache? cache = null
    )
    {
        cache ??= new RuleSpaceCache();

        return _inputProcessor.Match(input, firstSymbolIndex, cache);
    }

    public RuleMatchResultCollection MatchAndProject(
        RuleInput input,
        int firstSymbolIndex,
        RuleArguments ruleArguments,
        IRuleSpaceCache? cache = null
    )
    {
        var inputProcessorResult = Match(input, firstSymbolIndex, ruleArguments, cache);

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
                                input,
                                firstSymbolIndex,
                                ruleArguments,
                                _projection
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