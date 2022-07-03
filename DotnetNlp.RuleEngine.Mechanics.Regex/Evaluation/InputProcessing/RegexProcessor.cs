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

    public RegexProcessor(RegexAutomaton automaton, IRegexAutomatonWalker<RegexAutomaton> regexAutomatonWalker)
    {
        _automaton = automaton;
        _regexAutomatonWalker = regexAutomatonWalker;
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