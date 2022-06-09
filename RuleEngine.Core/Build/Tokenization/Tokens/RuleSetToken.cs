using System;
using System.Linq;
using RuleEngine.Core.Lib.CodeAnalysis.Tokenization.Tokens;
using RuleEngine.Core.Lib.Common.Helpers;

namespace RuleEngine.Core.Build.Tokenization.Tokens;

public sealed class RuleSetToken : IToken
{
    public UsingToken[] Usings { get; }
    public RuleToken[] Rules { get; }

    public RuleSetToken(UsingToken[] usings, RuleToken[] rules)
    {
        Usings = usings;
        Rules = rules;
    }

    public override string ToString()
    {
        return $"{(Usings.Length > 0 ? $"{Usings.Select(@using => @using.ToString()).JoinToString(Environment.NewLine)}{Environment.NewLine}{Environment.NewLine}" : "")}" +
               $"{Rules.Select(rule => rule.ToString()).JoinToString(Environment.NewLine)}";
    }
}