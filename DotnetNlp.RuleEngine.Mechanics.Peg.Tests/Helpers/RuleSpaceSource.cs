using System;
using System.Collections.Generic;
using System.Linq;
using DotnetNlp.RuleEngine.Core;
using DotnetNlp.RuleEngine.Core.Build.Tokenization.Tokens;
using DotnetNlp.RuleEngine.Core.Evaluation;
using DotnetNlp.RuleEngine.Core.Evaluation.Rule;
using DotnetNlp.RuleEngine.Core.Evaluation.Rule.Result.SelectionStrategy;
using DotnetNlp.RuleEngine.Core.Lib.CodeAnalysis.Assemblies;
using DotnetNlp.RuleEngine.Core.Lib.CodeAnalysis.Tokenization.Tokens;
using DotnetNlp.RuleEngine.Core.Lib.Common;
using DotnetNlp.RuleEngine.Core.Lib.Common.Helpers;
using DotnetNlp.RuleEngine.Mechanics.Peg.Build.InputProcessing;
using DotnetNlp.RuleEngine.Mechanics.Peg.Build.Tokenization;
using DotnetNlp.RuleEngine.Mechanics.Peg.Build.Tokenization.Tokens;

namespace DotnetNlp.RuleEngine.Mechanics.Peg.Tests.Helpers;

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