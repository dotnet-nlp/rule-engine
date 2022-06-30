using DotnetNlp.RuleEngine.Core;
using DotnetNlp.RuleEngine.Core.Build.Tokenization;
using DotnetNlp.RuleEngine.Core.Evaluation.Rule.Result.SelectionStrategy;
using DotnetNlp.RuleEngine.Core.Lib.Common;
using DotnetNlp.RuleEngine.Core.Lib.Common.Helpers;
using DotnetNlp.RuleEngine.Mechanics.Regex.Build.InputProcessing;
using DotnetNlp.RuleEngine.Mechanics.Regex.Build.InputProcessing.Automaton.Optimization;
using DotnetNlp.RuleEngine.Mechanics.Regex.Build.Tokenization;
using DotnetNlp.RuleEngine.Mechanics.Regex.Build.Tokenization.Tokens;

namespace DotnetNlp.RuleEngine.Mechanics.Regex.Tests.Helpers;

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

    public static MechanicsDescription RegexMechanics(OptimizationLevel optimizationLevel = OptimizationLevel.Min)
    {
        return new MechanicsDescription(
            "regex",
            Tokenizer,
            new RegexProcessorFactory(optimizationLevel),
            typeof(RegexGroupToken)
        );
    }
}