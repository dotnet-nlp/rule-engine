using System;
using DotnetNlp.RuleEngine.Core.Build.Tokenization.Tokens;
using DotnetNlp.RuleEngine.Core.Lib.CodeAnalysis.Tokenization.Tokens;
using DotnetNlp.RuleEngine.Core.Tests.Helpers;
using DotnetNlp.RuleEngine.Mechanics.Peg.Build.Tokenization.Tokens;

namespace DotnetNlp.RuleEngine.Core.Tests.Fixtures;

internal static class RepetitionOfLiteralReferenceTypeNer
{
    public static readonly RuleSetContainer Instance = new(
        "System.Collections.Generic.IReadOnlyCollection<System.String> ReferenceType_RepetitionOfLiteral = peg#(привет*:words)# { return words; }",
        new RuleSetToken(
            Array.Empty<UsingToken>(),
            new []
            {
                new RuleToken(
                    null,
                    new ClassicCSharpTypeToken(
                        new CSharpTypeNameWithNamespaceToken("System.Collections.Generic.IReadOnlyCollection"),
                        new ICSharpTypeToken[]
                        {
                            new ClassicCSharpTypeToken(new CSharpTypeNameWithNamespaceToken("System.String"), Array.Empty<ICSharpTypeToken>()),
                        }
                    ),
                    "ReferenceType_RepetitionOfLiteral",
                    Array.Empty<CSharpParameterToken>(),
                    "peg",
                    new PegGroupToken(
                        new []
                        {
                            new BranchToken(
                                new []
                                {
                                    new BranchItemToken(
                                        new LiteralToken("привет"),
                                        new QuantifierToken(0, null),
                                        "words",
                                        null
                                    ),
                                }
                            ),
                        }
                    ),
                    new BodyBasedProjectionToken("{ return words; }")
                ),
            }
        ),
        new []
        {
            StaticResources.PegMechanics(),
        }
    );
}