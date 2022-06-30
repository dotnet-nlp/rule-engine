using DotnetNlp.RuleEngine.Core;
using DotnetNlp.RuleEngine.Core.Evaluation.Rule.Result.SelectionStrategy;
using DotnetNlp.RuleEngine.Core.Lib.Common;
using DotnetNlp.RuleEngine.Core.Lib.Common.Helpers;
using DotnetNlp.RuleEngine.Mechanics.Peg.Build.InputProcessing;
using DotnetNlp.RuleEngine.Mechanics.Peg.Build.Tokenization;
using DotnetNlp.RuleEngine.Mechanics.Peg.Build.Tokenization.Tokens;
using DotnetNlp.RuleEngine.Mechanics.Regex.Build.InputProcessing;
using DotnetNlp.RuleEngine.Mechanics.Regex.Build.InputProcessing.Automaton.Optimization;
using DotnetNlp.RuleEngine.Mechanics.Regex.Build.Tokenization;
using DotnetNlp.RuleEngine.Mechanics.Regex.Build.Tokenization.Tokens;

namespace DotnetNlp.RuleEngine.Bundle;

public static class Mechanics
{
    private static readonly StringInterner StringInterner = new StringInterner();
    private static readonly ErrorIndexHelper ErrorIndexHelper = new ErrorIndexHelper("\r\n");

    public static readonly MechanicsDescription Peg = new MechanicsDescription(
        "peg",
        new LoopBasedPegPatternTokenizer(StringInterner, ErrorIndexHelper),
        new PegProcessorFactory(
            new CombinedStrategy(
                new []
                {
                    new MaxExplicitSymbolsStrategy(),
                    new MaxExplicitSymbolsStrategy(),
                }
            )
        ),
        typeof(PegGroupToken)
    );

    public static readonly MechanicsDescription Regex = new MechanicsDescription(
        "regex",
        new LoopBasedRegexPatternTokenizer(StringInterner, ErrorIndexHelper),
        new RegexProcessorFactory(OptimizationLevel.Max),
        typeof(RegexGroupToken)
    );
}