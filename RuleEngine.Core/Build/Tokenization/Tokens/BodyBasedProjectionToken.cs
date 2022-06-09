namespace RuleEngine.Core.Build.Tokenization.Tokens;

public sealed class BodyBasedProjectionToken : IProjectionToken
{
    public string Body { get; }

    public BodyBasedProjectionToken(string body)
    {
        Body = body;
    }

    public override string ToString()
    {
        return Body;
    }
}