using DotnetNlp.RuleEngine.Core.Build.Tokenization.Tokens;

namespace DotnetNlp.RuleEngine.Core.Tests.Helpers.Dummy;

internal sealed class DummyPatternToken : IPatternToken
{
    public string Pattern { get; }

    public DummyPatternToken(string pattern)
    {
        Pattern = pattern;
    }

    public override string ToString()
    {
        return Pattern;
    }
}