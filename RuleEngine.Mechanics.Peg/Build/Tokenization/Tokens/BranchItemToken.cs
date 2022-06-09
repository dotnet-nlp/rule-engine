using RuleEngine.Core.Lib.CodeAnalysis.Tokenization.Tokens;

namespace RuleEngine.Mechanics.Peg.Build.Tokenization.Tokens;

public sealed class BranchItemToken : IToken
{
    public IQuantifiableToken Quantifiable { get; }
    public QuantifierToken Quantifier { get; }
    public string? VariableName { get; }
    public LookaheadToken? Lookahead { get; }

    public BranchItemToken(
        IQuantifiableToken quantifiable,
        QuantifierToken quantifier,
        string? variableName,
        LookaheadToken? lookahead
    )
    {
        Quantifiable = quantifiable;
        Quantifier = quantifier;
        VariableName = variableName;
        Lookahead = lookahead;
    }

    public override string ToString()
    {
        return $"{Lookahead}" +
               $"{Quantifiable}" +
               $"{Quantifier}" +
               $"{(VariableName is not null ? $":{VariableName}" : "")}";
    }
}