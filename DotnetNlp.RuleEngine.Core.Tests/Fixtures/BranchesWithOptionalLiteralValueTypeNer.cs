using System;
using DotnetNlp.RuleEngine.Core.Build.Tokenization.Tokens;
using DotnetNlp.RuleEngine.Core.Build.Tokenization.Tokens.Arguments;
using DotnetNlp.RuleEngine.Core.Lib.CodeAnalysis.Tokenization.Tokens;
using DotnetNlp.RuleEngine.Core.Tests.Helpers;
using DotnetNlp.RuleEngine.Mechanics.Peg.Build.Tokenization.Tokens;

namespace DotnetNlp.RuleEngine.Core.Tests.Fixtures;

internal static class BranchesWithOptionalLiteralValueTypeNer
{
    public static readonly RuleSetContainer Instance = new(
        "\r\nusing System;\r\nusing DotnetNlp.RuleEngine.Core.Tests.Helpers;\r\n\r\nint Number_1 = peg#(один)# { return 1; }\r\nint Number_2 = peg#(два)# { return 2; }\r\nNullable<int> ValueType_BranchesWithOptionalLiteral = peg#(старт $Number_1?:n_1 финиш|старт $Number_2?:n_2 финиш)# { return n_1 ?? n_2; }",
        new RuleSetToken(
            new []
            {
                new UsingToken("System"),
                new UsingToken("DotnetNlp.RuleEngine.Core.Tests.Helpers"),
            },
            new []
            {
                new RuleToken(
                    null,
                    new ClassicCSharpTypeToken(
                        new CSharpTypeNameWithNamespaceToken("int"),
                        Array.Empty<ICSharpTypeToken>()
                    ),
                    "Number_1",
                    Array.Empty<CSharpParameterToken>(),
                    "peg",
                    new PegGroupToken(
                        new []
                        {
                            new BranchToken(
                                new []
                                {
                                    new BranchItemToken(
                                        new LiteralToken("один"),
                                        new QuantifierToken(1, 1),
                                        null,
                                        null
                                    ),
                                }
                            ),
                        }
                    ),
                    new BodyBasedProjectionToken("{ return 1; }")
                ),
                new RuleToken(
                    null,
                    new ClassicCSharpTypeToken(
                        new CSharpTypeNameWithNamespaceToken("int"),
                        Array.Empty<ICSharpTypeToken>()
                    ),
                    "Number_2",
                    Array.Empty<CSharpParameterToken>(),
                    "peg",
                    new PegGroupToken(
                        new []
                        {
                            new BranchToken(
                                new []
                                {
                                    new BranchItemToken(
                                        new LiteralToken("два"),
                                        new QuantifierToken(1, 1),
                                        null,
                                        null
                                    ),
                                }
                            ),
                        }
                    ),
                    new BodyBasedProjectionToken("{ return 2; }")
                ),
                new RuleToken(
                    null,
                    new ClassicCSharpTypeToken(
                        new CSharpTypeNameWithNamespaceToken("Nullable"),
                        new ICSharpTypeToken[]
                        {
                            new ClassicCSharpTypeToken(new CSharpTypeNameWithNamespaceToken("int"), Array.Empty<ICSharpTypeToken>()),
                        }
                    ),
                    "ValueType_BranchesWithOptionalLiteral",
                    Array.Empty<CSharpParameterToken>(),
                    "peg",
                    new PegGroupToken(
                        new []
                        {
                            new BranchToken(
                                new []
                                {
                                    new BranchItemToken(
                                        new LiteralToken("старт"),
                                        new QuantifierToken(1, 1),
                                        null,
                                        null
                                    ),
                                    new BranchItemToken(
                                        new RuleReferenceToken(null, "Number_1", Array.Empty<IRuleArgumentToken>()),
                                        new QuantifierToken(0, 1),
                                        "n_1",
                                        null
                                    ),
                                    new BranchItemToken(
                                        new LiteralToken("финиш"),
                                        new QuantifierToken(1, 1),
                                        null,
                                        null
                                    ),
                                }
                            ),
                            new BranchToken(
                                new []
                                {
                                    new BranchItemToken(
                                        new LiteralToken("старт"),
                                        new QuantifierToken(1, 1),
                                        null,
                                        null
                                    ),
                                    new BranchItemToken(
                                        new RuleReferenceToken(null, "Number_2", Array.Empty<IRuleArgumentToken>()),
                                        new QuantifierToken(0, 1),
                                        "n_2",
                                        null
                                    ),
                                    new BranchItemToken(
                                        new LiteralToken("финиш"),
                                        new QuantifierToken(1, 1),
                                        null,
                                        null
                                    ),
                                }
                            ),
                        }
                    ),
                    new BodyBasedProjectionToken("{ return n_1 ?? n_2; }")
                ),
            }
        ),
        new []
        {
            StaticResources.PegMechanics(),
        }
    );
}