﻿using System;
using System.Collections.Generic;
using DotnetNlp.RuleEngine.Core.Build.Tokenization.Tokens.Arguments;
using DotnetNlp.RuleEngine.Mechanics.Peg.Build.Tokenization.Tokens;
using DotnetNlp.RuleEngine.Mechanics.Peg.Tests.Helpers;

namespace DotnetNlp.RuleEngine.Mechanics.Peg.Tests.Fixtures;

internal static class SumOfNumbers0To9Grammar
{
    public static readonly RuleSpaceSource Instance = new(
        new Dictionary<string, (string Definition, PegGroupToken Token)>
        {
            {
                "Sum",
                (
                    "($Number (плюс $Sum)*)",
                    new PegGroupToken(
                        new []
                        {
                            new BranchToken(
                                new []
                                {
                                    new BranchItemToken(
                                        new RuleReferenceToken(null, "Number", Array.Empty<IRuleArgumentToken>()),
                                        new QuantifierToken(1, 1),
                                        null,
                                        null
                                    ),
                                    new BranchItemToken(
                                        new PegGroupToken(
                                            new []
                                            {
                                                new BranchToken(
                                                    new []
                                                    {
                                                        new BranchItemToken(
                                                            new LiteralToken("плюс"),
                                                            new QuantifierToken(1, 1),
                                                            null,
                                                            null
                                                        ),
                                                        new BranchItemToken(
                                                            new RuleReferenceToken(null, "Sum", Array.Empty<IRuleArgumentToken>()),
                                                            new QuantifierToken(1, 1),
                                                            null,
                                                            null
                                                        ),
                                                    }
                                                ),
                                            }
                                        ),
                                        new QuantifierToken(0, null),
                                        null,
                                        null
                                    ),
                                }
                            ),
                        }
                    )
                )
            },
            {
                "Number",
                (
                    "([ноль один два три четыре пять шесть семь восемь девять])",
                    new PegGroupToken(
                        new []
                        {
                            new BranchToken(
                                new []
                                {
                                    new BranchItemToken(
                                        new LiteralSetToken(
                                            false,
                                            new ILiteralSetMemberToken[]
                                            {
                                                new LiteralToken("ноль"),
                                                new LiteralToken("один"),
                                                new LiteralToken("два"),
                                                new LiteralToken("три"),
                                                new LiteralToken("четыре"),
                                                new LiteralToken("пять"),
                                                new LiteralToken("шесть"),
                                                new LiteralToken("семь"),
                                                new LiteralToken("восемь"),
                                                new LiteralToken("девять"),
                                            }
                                        ),
                                        new QuantifierToken(1, 1),
                                        null,
                                        null
                                    ),
                                }
                            ),
                        }
                    )
                )
            },
        }
    );
}