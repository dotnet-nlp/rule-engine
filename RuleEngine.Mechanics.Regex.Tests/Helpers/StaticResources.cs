using RuleEngine.Core;
using RuleEngine.Core.Build.Tokenization;
using RuleEngine.Core.Evaluation.Rule.Result.SelectionStrategy;
using RuleEngine.Core.Lib.Common;
using RuleEngine.Core.Lib.Common.Helpers;
using RuleEngine.Mechanics.Regex.Build.InputProcessing;
using RuleEngine.Mechanics.Regex.Build.InputProcessing.Automaton.Optimization;
using RuleEngine.Mechanics.Regex.Build.Tokenization;
using RuleEngine.Mechanics.Regex.Build.Tokenization.Tokens;

namespace RuleEngine.Mechanics.Regex.Tests.Helpers;

internal static class StaticResources
{
    public static readonly StringInterner StringInterner = new();
    public static readonly ErrorIndexHelper ErrorIndexHelper = new ErrorIndexHelper("\r\n");
    public static readonly IPatternTokenizer Tokenizer = new LoopBasedRegexPatternTokenizer(StringInterner, ErrorIndexHelper);
    public static readonly IResultSelectionStrategy ResultSelectionStrategy = new CombinedStrategy(
        new IResultSelectionStrategy[]
        {
            new MaxExplicitSymbolsStrategy(),
            new MaxProgressStrategy(),
        }
    );

    public static MechanicsBundle RegexMechanics(OptimizationLevel optimizationLevel = OptimizationLevel.Min)
    {
        return new MechanicsBundle(
            "regex",
            Tokenizer,
            new RegexProcessorFactory(optimizationLevel),
            typeof(RegexGroupToken)
        );
    }
}