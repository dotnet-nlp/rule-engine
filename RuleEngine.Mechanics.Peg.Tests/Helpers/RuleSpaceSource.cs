using System;
using System.Collections.Generic;
using System.Linq;
using RuleEngine.Core;
using RuleEngine.Core.Build.Tokenization.Tokens;
using RuleEngine.Core.Evaluation;
using RuleEngine.Core.Evaluation.Rule;
using RuleEngine.Core.Evaluation.Rule.Result.SelectionStrategy;
using RuleEngine.Core.Lib.CodeAnalysis.Assemblies;
using RuleEngine.Core.Lib.CodeAnalysis.Tokenization.Tokens;
using RuleEngine.Core.Lib.Common;
using RuleEngine.Core.Lib.Common.Helpers;
using RuleEngine.Mechanics.Peg.Build.InputProcessing;
using RuleEngine.Mechanics.Peg.Build.Tokenization;
using RuleEngine.Mechanics.Peg.Build.Tokenization.Tokens;

namespace RuleEngine.Mechanics.Peg.Tests.Helpers;

internal sealed class RuleSpaceSource
{
    private static readonly RuleSpaceFactory Factory = new(
        new[]
        {
            new MechanicsBundle(
                "peg",
                new LoopBasedPegPatternTokenizer(new StringInterner(), new ErrorIndexHelper("\r\n")),
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
            ),
        }
    );

    private readonly IReadOnlyDictionary<string, (string Definition, PegGroupToken Token)> _rules;

    private IRuleSpace? _ruleSpace;
    public IRuleSpace RuleSpace => _ruleSpace ??= CreateRuleSpace();

    public RuleSpaceSource(IReadOnlyDictionary<string, (string Definition, PegGroupToken Token)> rules)
    {
        _rules = rules;
    }

    private IRuleSpace CreateRuleSpace()
    {
        return Factory.CreateWithAliases(
            Array.Empty<RuleSetToken>(),
            _rules
                .MapValue(
                    (ruleName, rulePattern) => new RuleToken(
                        null,
                        new ClassicCSharpTypeToken(new CSharpTypeNameWithNamespaceToken("void"), Array.Empty<ICSharpTypeToken>()),
                        ruleName,
                        Array.Empty<CSharpParameterToken>(),
                        "peg",
                        rulePattern.Token,
                        VoidProjectionToken.Instance
                    )
                )
                .SelectValues()
                .ToArray(),
            new Dictionary<string, IRuleMatcher>(),
            new Dictionary<string, IRuleSpace>(),
            new Dictionary<string, Type>(),
            new LoadedAssembliesProvider()
        );
    }
}