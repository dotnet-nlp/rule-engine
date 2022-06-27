using System;
using DotnetNlp.RuleEngine.Core.Build.Tokenization.Tokens;
using DotnetNlp.RuleEngine.Core.Lib.CodeAnalysis.Tokenization.Tokens;
using DotnetNlp.RuleEngine.Core.Tests.Helpers;
using DotnetNlp.RuleEngine.Mechanics.Peg.Build.Tokenization.Tokens;

namespace DotnetNlp.RuleEngine.Core.Tests.Fixtures;

internal static class BranchesWithRepetitionOfLiteralReferenceTypeNer
{
    public static readonly RuleSetContainer Instance = new(
        "System.Collections.Generic.IReadOnlyCollection<string> ReferenceType_BranchesWithRepetitionOfLiteral = peg#(привет+:words_1|пока+:words_2)# { return words_1 ?? words_2; }",
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
                            new ClassicCSharpTypeToken(new CSharpTypeNameWithNamespaceToken("string"), Array.Empty<ICSharpTypeToken>()),
                        }
                    ),
                    "ReferenceType_BranchesWithRepetitionOfLiteral",
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
                                        new QuantifierToken(1, null),
                                        "words_1",
                                        null
                                    ),
                                }
                            ),
                            new BranchToken(
                                new []
                                {
                                    new BranchItemToken(
                                        new LiteralToken("пока"),
                                        new QuantifierToken(1, null),
                                        "words_2",
                                        null
                                    ),
                                }
                            ),
                        }
                    ),
                    new BodyBasedProjectionToken("{ return words_1 ?? words_2; }")
                ),
            }
        ),
        new []
        {
            NerEnvironment.Mechanics.Peg,
        }
    );
}