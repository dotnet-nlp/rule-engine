using System;
using DotnetNlp.RuleEngine.Core.Build.Tokenization.Tokens;
using DotnetNlp.RuleEngine.Core.Build.Tokenization.Tokens.Arguments;
using DotnetNlp.RuleEngine.Core.Lib.CodeAnalysis.Tokenization.Tokens;
using DotnetNlp.RuleEngine.Core.Tests.Helpers;
using DotnetNlp.RuleEngine.Mechanics.Peg.Build.Tokenization.Tokens;

namespace DotnetNlp.RuleEngine.Core.Tests.Fixtures;

internal static class RuleArgumentsExampleNer
{
    public static readonly RuleSetContainer Instance = new(
        "\r\nstring DummyRuleWithArguments(string arg1, int arg2) = peg#(.:word)# { return $\"{word}_arg1<{arg1}>_arg2<{arg2}>\"; }\r\n\r\nstring DummyRuleWithReferenceToRuleWithArguments1 = peg#($DummyRuleWithArguments(parameters.Foo, parameters.Bar):word)# { return word; }\r\nstring DummyRuleWithReferenceToRuleWithArguments2 = peg#($DummyRuleWithArguments(default, parameters.Bar):word)# { return word; }\r\nstring DummyRuleWithReferenceToRuleWithArguments3 = peg#($DummyRuleWithArguments(parameters.Foo, default):word)# { return word; }\r\nstring DummyRuleWithReferenceToRuleWithArguments4 = peg#($DummyRuleWithArguments(default, default):word)# { return word; }\r\n\r\n",
        new RuleSetToken(
            Array.Empty<UsingToken>(),
            new []
            {
                new RuleToken(
                    null,
                    new ClassicCSharpTypeToken(new CSharpTypeNameWithNamespaceToken("string"), Array.Empty<ICSharpTypeToken>()),
                    "DummyRuleWithArguments",
                    new []
                    {
                        new CSharpParameterToken(
                            new ClassicCSharpTypeToken(new CSharpTypeNameWithNamespaceToken("string"), Array.Empty<ICSharpTypeToken>()),
                            "arg1"
                        ),
                        new CSharpParameterToken(
                            new ClassicCSharpTypeToken(new CSharpTypeNameWithNamespaceToken("int"), Array.Empty<ICSharpTypeToken>()),
                            "arg2"
                        ),
                    },
                    "peg",
                    new PegGroupToken(
                        new []
                        {
                            new BranchToken(
                                new []
                                {
                                    new BranchItemToken(
                                        AnyLiteralToken.Instance,
                                        new QuantifierToken(1, 1),
                                        "word",
                                        null
                                    ),
                                }
                            ),
                        }
                    ),
                    new BodyBasedProjectionToken("{ return $\"{word}_arg1<{arg1}>_arg2<{arg2}>\"; }")
                ),
                new RuleToken(
                    null,
                    new ClassicCSharpTypeToken(new CSharpTypeNameWithNamespaceToken("string"), Array.Empty<ICSharpTypeToken>()),
                    "DummyRuleWithReferenceToRuleWithArguments1",
                    Array.Empty<CSharpParameterToken>(),
                    "peg",
                    new PegGroupToken(
                        new []
                        {
                            new BranchToken(
                                new []
                                {
                                    new BranchItemToken(
                                        new RuleReferenceToken(
                                            null,
                                            "DummyRuleWithArguments",
                                            new IRuleArgumentToken[]
                                            {
                                                new RuleChainedMemberAccessArgumentToken(
                                                    new []
                                                    {
                                                        "parameters",
                                                        "Foo",
                                                    }
                                                ),
                                                new RuleChainedMemberAccessArgumentToken(
                                                    new []
                                                    {
                                                        "parameters",
                                                        "Bar",
                                                    }
                                                ),
                                            }
                                        ),
                                        new QuantifierToken(1, 1),
                                        "word",
                                        null
                                    ),
                                }
                            ),
                        }
                    ),
                    new BodyBasedProjectionToken("{ return word; }")
                ),
                new RuleToken(
                    null,
                    new ClassicCSharpTypeToken(new CSharpTypeNameWithNamespaceToken("string"), Array.Empty<ICSharpTypeToken>()),
                    "DummyRuleWithReferenceToRuleWithArguments2",
                    Array.Empty<CSharpParameterToken>(),
                    "peg",
                    new PegGroupToken(
                        new []
                        {
                            new BranchToken(
                                new []
                                {
                                    new BranchItemToken(
                                        new RuleReferenceToken(
                                            null,
                                            "DummyRuleWithArguments",
                                            new IRuleArgumentToken[]
                                            {
                                                RuleDefaultArgumentToken.Instance,
                                                new RuleChainedMemberAccessArgumentToken(
                                                    new []
                                                    {
                                                        "parameters",
                                                        "Bar",
                                                    }
                                                ),
                                            }
                                        ),
                                        new QuantifierToken(1, 1),
                                        "word",
                                        null
                                    ),
                                }
                            ),
                        }
                    ),
                    new BodyBasedProjectionToken("{ return word; }")
                ),
                new RuleToken(
                    null,
                    new ClassicCSharpTypeToken(new CSharpTypeNameWithNamespaceToken("string"), Array.Empty<ICSharpTypeToken>()),
                    "DummyRuleWithReferenceToRuleWithArguments3",
                    Array.Empty<CSharpParameterToken>(),
                    "peg",
                    new PegGroupToken(
                        new []
                        {
                            new BranchToken(
                                new []
                                {
                                    new BranchItemToken(
                                        new RuleReferenceToken(
                                            null,
                                            "DummyRuleWithArguments",
                                            new IRuleArgumentToken[]
                                            {
                                                new RuleChainedMemberAccessArgumentToken(
                                                    new []
                                                    {
                                                        "parameters",
                                                        "Foo",
                                                    }
                                                ),
                                                RuleDefaultArgumentToken.Instance,
                                            }
                                        ),
                                        new QuantifierToken(1, 1),
                                        "word",
                                        null
                                    ),
                                }
                            ),
                        }
                    ),
                    new BodyBasedProjectionToken("{ return word; }")
                ),
                new RuleToken(
                    null,
                    new ClassicCSharpTypeToken(new CSharpTypeNameWithNamespaceToken("string"), Array.Empty<ICSharpTypeToken>()),
                    "DummyRuleWithReferenceToRuleWithArguments4",
                    Array.Empty<CSharpParameterToken>(),
                    "peg",
                    new PegGroupToken(
                        new []
                        {
                            new BranchToken(
                                new []
                                {
                                    new BranchItemToken(
                                        new RuleReferenceToken(
                                            null,
                                            "DummyRuleWithArguments",
                                            new IRuleArgumentToken[]
                                            {
                                                RuleDefaultArgumentToken.Instance,
                                                RuleDefaultArgumentToken.Instance,
                                            }
                                        ),
                                        new QuantifierToken(1, 1),
                                        "word",
                                        null
                                    ),
                                }
                            ),
                        }
                    ),
                    new BodyBasedProjectionToken("{ return word; }")
                ),
            }
        ),
        new []
        {
            StaticResources.RegexMechanics(),
            StaticResources.PegMechanics(),
        }
    );
}