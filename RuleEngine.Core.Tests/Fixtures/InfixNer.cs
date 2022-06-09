using System;
using RuleEngine.Core.Build.Tokenization.Tokens;
using RuleEngine.Core.Lib.CodeAnalysis.Tokenization.Tokens;
using RuleEngine.Core.Tests.Helpers;
using RuleEngine.Mechanics.Peg.Build.Tokenization.Tokens;

namespace RuleEngine.Core.Tests.Fixtures;

internal static class InfixNer
{
    public static readonly RuleSetContainer Instance = new(
        "System.String ReferenceType_Infix = peg#(~ству~:word)# { return word; }",
        new RuleSetToken(
            Array.Empty<UsingToken>(),
            new []
            {
                new RuleToken(
                    null,
                    new ClassicCSharpTypeToken(new CSharpTypeNameWithNamespaceToken("System.String"), Array.Empty<ICSharpTypeToken>()),
                    "ReferenceType_Infix",
                    Array.Empty<CSharpParameterToken>(),
                    "peg",
                    new PegGroupToken(
                        new []
                        {
                            new BranchToken(
                                new []
                                {
                                    new BranchItemToken(
                                        new InfixToken("ству"),
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