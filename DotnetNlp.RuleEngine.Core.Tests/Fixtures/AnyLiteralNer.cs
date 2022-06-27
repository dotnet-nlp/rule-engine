using System;
using DotnetNlp.RuleEngine.Core.Build.Tokenization.Tokens;
using DotnetNlp.RuleEngine.Core.Lib.CodeAnalysis.Tokenization.Tokens;
using DotnetNlp.RuleEngine.Core.Tests.Helpers;
using DotnetNlp.RuleEngine.Mechanics.Peg.Build.Tokenization.Tokens;

namespace DotnetNlp.RuleEngine.Core.Tests.Fixtures;

internal static class AnyLiteralNer
{
    public static readonly RuleSetContainer Instance = new(
        "System.String ReferenceType_AnyLiteral = peg#(.:word)# { return word; }",
        new RuleSetToken(
            Array.Empty<UsingToken>(),
            new []
            {
                new RuleToken(
                    null,
                    new ClassicCSharpTypeToken(new CSharpTypeNameWithNamespaceToken("System.String"), Array.Empty<ICSharpTypeToken>()),
                    "ReferenceType_AnyLiteral",
                    Array.Empty<CSharpParameterToken>(),
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
                    new BodyBasedProjectionToken("{ return word; }")
                ),
            }
        ),
        new []
        {
            NerEnvironment.Mechanics.Peg,
        }
    );
}