using System;
using System.Collections.Generic;
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

internal static class StaticResources
{
    public static readonly StringInterner StringInterner = new();
    public static readonly ErrorIndexHelper ErrorIndexHelper = new ErrorIndexHelper("\r\n");
    public static readonly IResultSelectionStrategy ResultSelectionStrategy = new CombinedStrategy(
        new IResultSelectionStrategy[]
        {
            new MaxExplicitSymbolsStrategy(),
            new MaxProgressStrategy(),
        }
    );

    public static readonly IPatternTokenizer RegexTokenizer = new LoopBasedRegexPatternTokenizer(StringInterner, ErrorIndexHelper);
    public static readonly IPatternTokenizer PegTokenizer = new LoopBasedPegPatternTokenizer(StringInterner, ErrorIndexHelper);
    public static readonly LoopBasedRuleSetTokenizer RuleSetTokenizer = new (
        new Dictionary<string, IPatternTokenizer>
        {
            {PegMechanics().Key, PegMechanics().Tokenizer},
            {RegexMechanics().Key, RegexMechanics().Tokenizer},
        },
        ErrorIndexHelper
    );
    public static readonly LoopBasedRuleSetTokenizer DummyRuleSetTokenizer = new(
        new Dictionary<string, IPatternTokenizer>
        {
            {DummyMechanics().Key, DummyMechanics().Tokenizer},
        },
        ErrorIndexHelper
    );

    public static MechanicsDescription RegexMechanics(OptimizationLevel optimizationLevel = OptimizationLevel.Min)
    {
        return new MechanicsDescription(
            "regex",
            RegexTokenizer,
            new RegexProcessorFactory(optimizationLevel),
            typeof(RegexGroupToken)
        );
    }

    public static MechanicsDescription PegMechanics(IResultSelectionStrategy? resultSelectionStrategy = null)
    {
        return new MechanicsDescription(
            "peg",
            PegTokenizer,
            new PegProcessorFactory(resultSelectionStrategy ?? ResultSelectionStrategy),
            typeof(PegGroupToken)
        );
    }

    
    public static MechanicsDescription DummyMechanics()
    {
        return new MechanicsDescription(
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