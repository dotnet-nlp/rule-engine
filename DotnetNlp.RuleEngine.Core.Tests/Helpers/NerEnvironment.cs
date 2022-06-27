using System;
using DotnetNlp.RuleEngine.Core.Build;
using DotnetNlp.RuleEngine.Core.Build.InputProcessing;
using DotnetNlp.RuleEngine.Core.Build.InputProcessing.Models;
using DotnetNlp.RuleEngine.Core.Build.Tokenization;
using DotnetNlp.RuleEngine.Core.Build.Tokenization.Tokens;
using DotnetNlp.RuleEngine.Core.Evaluation;
using DotnetNlp.RuleEngine.Core.Evaluation.InputProcessing;
using DotnetNlp.RuleEngine.Core.Evaluation.Rule.Result.SelectionStrategy;
using DotnetNlp.RuleEngine.Core.Lib.Common;
using DotnetNlp.RuleEngine.Core.Lib.Common.Helpers;
using DotnetNlp.RuleEngine.Core.Tests.Helpers.Dummy;
using DotnetNlp.RuleEngine.Mechanics.Peg.Build.InputProcessing;
using DotnetNlp.RuleEngine.Mechanics.Peg.Build.Tokenization;
using DotnetNlp.RuleEngine.Mechanics.Peg.Build.Tokenization.Tokens;
using DotnetNlp.RuleEngine.Mechanics.Regex.Build.InputProcessing;
using DotnetNlp.RuleEngine.Mechanics.Regex.Build.InputProcessing.Automaton.Optimization;
using DotnetNlp.RuleEngine.Mechanics.Regex.Build.Tokenization;
using DotnetNlp.RuleEngine.Mechanics.Regex.Build.Tokenization.Tokens;

namespace DotnetNlp.RuleEngine.Core.Tests.Helpers;

public static class NerEnvironment
{
    public static readonly ErrorIndexHelper ErrorIndexHelper = new("\r\n");

    public static class Mechanics
    {
        private static readonly StringInterner StringInterner = new();

        public static readonly MechanicsBundle Peg = new(
            "peg",
            new LoopBasedPegPatternTokenizer(StringInterner, ErrorIndexHelper),
            new PegProcessorFactory(
                new CombinedStrategy(
                    new IResultSelectionStrategy[]
                    {
                        new MaxExplicitSymbolsStrategy(),
                        new MaxProgressStrategy(),
                    }
                )
            ),
            typeof(PegGroupToken)
        );

        public static readonly MechanicsBundle Regex = new(
            "regex",
            new LoopBasedRegexPatternTokenizer(StringInterner, ErrorIndexHelper),
            new RegexProcessorFactory(OptimizationLevel.Max),
            typeof(RegexGroupToken)
        );

        public static MechanicsBundle Dummy { get; } = new(
            "dummy",
            new DummyTokenizer(),
            new DummyProcessorFactory(),
            typeof(DummyPatternToken)
        );
    }

    private sealed class DummyTokenizer : IPatternTokenizer
    {
        public IPatternToken Tokenize(string pattern, string? @namespace, bool caseSensitive)
        {
            return new DummyPatternToken(pattern);
        }
    }

    private sealed class DummyProcessorFactory : IInputProcessorFactory
    {
        public IInputProcessor Create(
            IPatternToken patternToken,
            IRuleSpace ruleSpace,
            IRuleSpaceDescription ruleSpaceDescription
        )
        {
            throw new Exception("Wrong usage");
        }

        public RuleCapturedVariables ExtractOwnCapturedVariables(
            IPatternToken patternToken,
            IRuleSpaceDescription ruleSpaceDescription
        )
        {
            throw new Exception("Wrong usage");
        }
    }
}