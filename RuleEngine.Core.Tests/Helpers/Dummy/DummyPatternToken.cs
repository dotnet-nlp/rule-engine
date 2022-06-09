using RuleEngine.Core.Build.Tokenization.Tokens;

namespace RuleEngine.Core.Tests.Helpers.Dummy;

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