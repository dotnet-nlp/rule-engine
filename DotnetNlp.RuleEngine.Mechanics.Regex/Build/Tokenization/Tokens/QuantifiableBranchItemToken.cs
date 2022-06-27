namespace DotnetNlp.RuleEngine.Mechanics.Regex.Build.Tokenization.Tokens;

public sealed class QuantifiableBranchItemToken : IBranchItemToken
{
    public IQuantifiableToken Quantifiable { get; }
    public QuantifierToken Quantifier { get; }
    public string? VariableName { get; }

    public QuantifiableBranchItemToken(
        IQuantifiableToken quantifiable,
        QuantifierToken quantifier,
        string? variableName
    )
    {
        Quantifiable = quantifiable;
        Quantifier = quantifier;
        VariableName = variableName;
    }

    public override string ToString()
    {
        return $"{Quantifiable}" +
               $"{Quantifier}" +
               $"{(VariableName is not null ? $":{VariableName}" : "")}";
    }
}