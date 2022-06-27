namespace DotnetNlp.RuleEngine.Core.Build.Tokenization.Tokens;

public sealed class ConstantProjectionToken : IProjectionToken
{
    public object? Constant { get; }

    public ConstantProjectionToken(object? constant)
    {
        Constant = constant;
    }

    public override string ToString()
    {
        return $"=> {Format()}";

        string Format()
        {
            return Constant switch
            {
                string => @$"""{Constant}""",
                _ => $"{Constant}",
            };
        }
    }
}