using System;
using RuleEngine.Core.Build.Tokenization.Tokens;
using RuleEngine.Core.Build.Tokenization.Tokens.Arguments;
using RuleEngine.Core.Lib.CodeAnalysis.Tokenization.Tokens;
using RuleEngine.Core.Tests.Helpers;
using RuleEngine.Mechanics.Peg.Build.Tokenization.Tokens;

namespace RuleEngine.Core.Tests.Fixtures;

internal static class RepetitionOfLiteralValueTypeNer
{
    public static readonly RuleSetContainer Instance = new(
        "\r\nusing System.Collections.Generic;\r\n\r\nint Number_1 = peg#(один)# { return 1; }\r\nSystem.Collections.Generic.IReadOnlyCollection<System.Int32> ValueType_RepetitionOfLiteral = peg#($Number_1*:numbers)# { return numbers; }\r\n",
        new RuleSetToken(
            new []
            {
                new UsingToken("System.Collections.Generic"),
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
                        new CSharpTypeNameWithNamespaceToken("System.Collections.Generic.IReadOnlyCollection"),
                        new ICSharpTypeToken[]
                        {
                            new ClassicCSharpTypeToken(new CSharpTypeNameWithNamespaceToken("System.Int32"), Array.Empty<ICSharpTypeToken>()),
                        }
                    ),
                    "ValueType_RepetitionOfLiteral",
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
                                        new QuantifierToken(0, null),
                                        "numbers",
                                        null
                                    ),
                                }
                            ),
                        }
                    ),
                    new BodyBasedProjectionToken("{ return numbers; }")
                ),
            }
        ),
        new []
        {
            NerEnvironment.Mechanics.Peg,
        }
    );
}