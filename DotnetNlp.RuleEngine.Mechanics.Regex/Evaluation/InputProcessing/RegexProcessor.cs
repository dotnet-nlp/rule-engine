using System.Collections.Generic;
using DotnetNlp.RuleEngine.Core.Build.Composition;
using DotnetNlp.RuleEngine.Core.Evaluation.Cache;
using DotnetNlp.RuleEngine.Core.Evaluation.InputProcessing;
using DotnetNlp.RuleEngine.Core.Evaluation.Rule.Projection.Arguments;
using DotnetNlp.RuleEngine.Core.Evaluation.Rule.Result;
using DotnetNlp.RuleEngine.Mechanics.Regex.Evaluation.InputProcessing.Automaton.Models;
using DotnetNlp.RuleEngine.Mechanics.Regex.Evaluation.InputProcessing.Automaton.Walker;

namespace DotnetNlp.RuleEngine.Mechanics.Regex.Evaluation.InputProcessing;

internal sealed class RegexProcessor : IInputProcessor
{
    public IRuleDependenciesProvider DependenciesProvider { get; }
    private readonly RegexAutomaton _automaton;

    public RegexProcessor(IRuleDependenciesProvider dependenciesProvider, RegexAutomaton automaton)
    {
        DependenciesProvider = dependenciesProvider;
        _automaton = automaton;
    }

    public RuleMatchResultCollection Match(
        string[] sequence,
        int firstSymbolIndex = 0,
        RuleSpaceArguments? ruleSpaceArguments = null,
        IRuleSpaceCache? cache = null
    )
    {
        return RegexAutomatonWalker.Instance.Walk(_automaton, sequence, firstSymbolIndex, ruleSpaceArguments, cache);
    }

    public IEnumerable<string> GetUsedWords()
    {
        return _automaton.GetUsedWords();
    }
}