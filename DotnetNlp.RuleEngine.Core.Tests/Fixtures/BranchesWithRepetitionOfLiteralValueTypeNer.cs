using System;
using DotnetNlp.RuleEngine.Core.Build.Tokenization.Tokens;
using DotnetNlp.RuleEngine.Core.Build.Tokenization.Tokens.Arguments;
using DotnetNlp.RuleEngine.Core.Lib.CodeAnalysis.Tokenization.Tokens;
using DotnetNlp.RuleEngine.Core.Tests.Helpers;
using DotnetNlp.RuleEngine.Mechanics.Peg.Build.Tokenization.Tokens;

namespace DotnetNlp.RuleEngine.Core.Tests.Fixtures;

internal static class BranchesWithRepetitionOfLiteralValueTypeNer
{
    public static readonly RuleSetContainer Instance = new(
        "\r\nusing System.Linq;\r\n\r\nint Number_1 = peg#(один)# { return 1; }\r\nint Number_2 = peg#(два)# { return 2; }\r\nSystem.Collections.Generic.IReadOnlyCollection<int> ValueType_BranchesWithRepetitionOfLiteral = peg#($Number_1+:n_1|$Number_2+:n_2)# { return n_1 ?? n_2; }\r\n",
        new RuleSetToken(
            new []
            {
                new UsingToken("System.Linq"),
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
                        new CSharpTypeNameWithNamespaceToken("System.Collections.Generic.IReadOnlyCollection"),
                        new ICSharpTypeToken[]
                        {
                            new ClassicCSharpTypeToken(new CSharpTypeNameWithNamespaceToken("int"), Array.Empty<ICSharpTypeToken>()),
                        }
                    ),
                    "ValueType_BranchesWithRepetitionOfLiteral",
                    Array.Empty<CSharpParameterToken>(),
                    "peg",
                    new PegGroupToken(
                        new []
                        {
                            new BranchToken(
                                new []
                                {
                                    new BranchItemToken(
                                        new RuleReferenceToken(null, "Number_1", Array.Empty<IRuleArgumentToken>()),
                                        new QuantifierToken(1, null),
                                        "n_1",
                                        null
                                    ),
                                }
                            ),
                            new BranchToken(
                                new []
                                {
                                    new BranchItemToken(
                                        new RuleReferenceToken(null, "Number_2", Array.Empty<IRuleArgumentToken>()),
                                        new QuantifierToken(1, null),
                                        "n_2",
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
            NerEnvironment.Mechanics.Peg,
        }
    );
}