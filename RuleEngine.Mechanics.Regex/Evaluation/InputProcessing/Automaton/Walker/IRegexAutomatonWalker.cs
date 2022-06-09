using RuleEngine.Core.Evaluation.Cache;
using RuleEngine.Core.Evaluation.Rule.Input;
using RuleEngine.Core.Evaluation.Rule.Result;

namespace RuleEngine.Mechanics.Regex.Evaluation.InputProcessing.Automaton.Walker;

internal interface IRegexAutomatonWalker<in TAutomaton>
{
    RuleMatchResultCollection Walk(
        TAutomaton automaton,
        RuleInput ruleInput,
        int firstSymbolIndex,
        IRuleSpaceCache cache
    );
}