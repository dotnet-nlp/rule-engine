using DotnetNlp.RuleEngine.Core.Evaluation.Cache;
using DotnetNlp.RuleEngine.Core.Evaluation.Rule.Input;
using DotnetNlp.RuleEngine.Core.Evaluation.Rule.Result;

namespace DotnetNlp.RuleEngine.Mechanics.Regex.Evaluation.InputProcessing.Automaton.Walker;

internal interface IRegexAutomatonWalker<in TAutomaton>
{
    RuleMatchResultCollection Walk(
        TAutomaton automaton,
        RuleInput ruleInput,
        int firstSymbolIndex,
        IRuleSpaceCache cache
    );
}