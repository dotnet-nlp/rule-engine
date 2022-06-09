using System.Collections.Generic;
using RuleEngine.Core.Evaluation.Cache;
using RuleEngine.Core.Evaluation.InputProcessing;
using RuleEngine.Core.Evaluation.Rule.Input;
using RuleEngine.Core.Evaluation.Rule.Result;
using RuleEngine.Mechanics.Regex.Evaluation.InputProcessing.Automaton.Models;
using RuleEngine.Mechanics.Regex.Evaluation.InputProcessing.Automaton.Walker;

namespace RuleEngine.Mechanics.Regex.Evaluation.InputProcessing;

internal sealed class RegexProcessor : IInputProcessor
{
    private readonly RegexAutomaton _automaton;
    private readonly IRegexAutomatonWalker<RegexAutomaton> _regexAutomatonWalker;

    public RegexProcessor(RegexAutomaton automaton, IRegexAutomatonWalker<RegexAutomaton> regexAutomatonWalker)
    {
        _automaton = automaton;
        _regexAutomatonWalker = regexAutomatonWalker;
    }

    public RuleMatchResultCollection Match(RuleInput ruleInput, int firstSymbolIndex, IRuleSpaceCache cache)
    {
        return _regexAutomatonWalker.Walk(_automaton, ruleInput, firstSymbolIndex, cache);
    }

    public IEnumerable<string> GetUsedWords()
    {
        return _automaton.GetUsedWords();
    }
}