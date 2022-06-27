namespace DotnetNlp.RuleEngine.Core.Lib.CodeAnalysis.Tokenization.Tokens.Internal;

public sealed class ContainerToken<TValue> : IToken
{
    public TValue Value { get; }

    public ContainerToken(TValue value)
    {
        Value = value;
    }

    public override string ToString()
    {
        return Value?.ToString() ?? "";
    }
}