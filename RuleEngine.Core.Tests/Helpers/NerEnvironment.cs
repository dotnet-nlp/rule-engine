using System;
using RuleEngine.Core.Build;
using RuleEngine.Core.Build.InputProcessing;
using RuleEngine.Core.Build.InputProcessing.Models;
using RuleEngine.Core.Build.Tokenization;
using RuleEngine.Core.Build.Tokenization.Tokens;
using RuleEngine.Core.Evaluation;
using RuleEngine.Core.Evaluation.InputProcessing;
using RuleEngine.Core.Evaluation.Rule.Result.SelectionStrategy;
using RuleEngine.Core.Lib.Common;
using RuleEngine.Core.Lib.Common.Helpers;
using RuleEngine.Core.Tests.Helpers.Dummy;
using RuleEngine.Mechanics.Peg.Build.InputProcessing;
using RuleEngine.Mechanics.Peg.Build.Tokenization;
using RuleEngine.Mechanics.Peg.Build.Tokenization.Tokens;
using RuleEngine.Mechanics.Regex.Build.InputProcessing;
using RuleEngine.Mechanics.Regex.Build.InputProcessing.Automaton.Optimization;
using RuleEngine.Mechanics.Regex.Build.Tokenization;
using RuleEngine.Mechanics.Regex.Build.Tokenization.Tokens;

namespace RuleEngine.Core.Tests.Helpers;

public static class NerEnvironment
{
    public static class Mechanics
    {
        private static readonly StringInterner StringInterner = new();
        private static readonly ErrorIndexHelper ErrorIndexHelper = new("\r\n");

        public static readonly MechanicsBundle Peg = new(
            "peg",
            new LoopBasedPegPatternTokenizer(StringInterner),
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