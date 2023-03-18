using System.Collections.Generic;
using DotnetNlp.RuleEngine.Core.Evaluation.Cache;
using DotnetNlp.RuleEngine.Core.Evaluation.InputProcessing;
using DotnetNlp.RuleEngine.Core.Evaluation.Rule.Projection.Arguments;
using DotnetNlp.RuleEngine.Core.Evaluation.Rule.Result;
using DotnetNlp.RuleEngine.Mechanics.Regex.Evaluation.InputProcessing.Automaton.Models;
using DotnetNlp.RuleEngine.Mechanics.Regex.Evaluation.InputProcessing.Automaton.Walker;

namespace DotnetNlp.RuleEngine.Mechanics.Regex.Evaluation.InputProcessing;

internal sealed class RegexProcessor : IInputProcessor
{
    private readonly RegexAutomaton _automaton;
    private readonly IRegexAutomatonWalker<RegexAutomaton> _regexAutomatonWalker;
    private readonly IReadOnlySet<string> _dependencies;

    public RegexProcessor(
        RegexAutomaton automaton,
        IRegexAutomatonWalker<RegexAutomaton> regexAutomatonWalker,
        IReadOnlySet<string> dependencies
    )
    {
        _automaton = automaton;
        _regexAutomatonWalker = regexAutomatonWalker;
        _dependencies = dependencies;
    }

    public IReadOnlySet<string> GetDependencies()
    {
        return _dependencies;
    }

    public RuleMatchResultCollection Match(
        string[] sequence,
        int firstSymbolIndex = 0,
        RuleSpaceArguments? ruleSpaceArguments = null,
        IRuleSpaceCache? cache = null
    )
    {
        return _regexAutomatonWalker.Walk(_automaton, sequence, firstSymbolIndex, ruleSpaceArguments, cache);
    }

    public IEnumerable<string> GetUsedWords()
    {
        return _automaton.GetUsedWords();
    }
}